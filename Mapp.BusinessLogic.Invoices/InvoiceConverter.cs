using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Shmap.BusinessLogic.Currency;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Mapp;
using Shmap.BusinessLogic.Transactions;

namespace Shmap.BusinessLogic.Invoices
{

    public class InvoiceConversionContext
    {
        public uint ExistingInvoiceNumber { get; init; }
        public string DefaultEmail { get; init; }
        public DateTime ConvertToDate { get; init; }
    }

    public interface IInvoiceConverter
    {
        IEnumerable<Invoice> LoadAmazonReports(IEnumerable<string> reportsFileNames, InvoiceConversionContext conversionContext);

        void ProcessInvoices(IEnumerable<Invoice> invoices, string fileName);
    }

    public class InvoiceConverter : IInvoiceConverter
    {
        public readonly IAutocompleteData _autocompleteData;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly IInvoicesXmlManager _invoicesXmlManager;
        private readonly IAutocompleteDataLoader _autocompleteDataLoader;
        private readonly IDialogService _dialogService;
        private static readonly int MaxItemNameLength = 85;
        private readonly int MaxAddressNameLength = 60;
        private readonly int MaxCityLength = 45;
        private readonly int MaxClientNameLength = 30;
        private Dictionary<string, decimal> Rates;
        private Dictionary<string, decimal> VatPercentage;

        public string[] _euContries = // TODO from settings
        {
            "BE", "BG", "DK", "EE", "FI", "FR", "IE", "IT", "CY", "LT", "LV", "LU", "HU", "HR", "MT", "DE",
            "NL", "PL", "PT", "AT", "RO", "GR", "SK", "SI", "ES", "SE", "EU", "CZ"
        };
  
        public InvoiceConverter(IAutocompleteData autocompleteData, 
            ICurrencyConverter currencyConverter,
            ICsvLoader csvLoader, 
            IInvoicesXmlManager invoicesXmlManager,
            IAutocompleteDataLoader autocompleteDataLoader,
            IDialogService dialogService)
        {
            _autocompleteData = autocompleteData;
            _currencyConverter = currencyConverter;
            _invoicesXmlManager = invoicesXmlManager;
            _autocompleteDataLoader = autocompleteDataLoader;
            _dialogService = dialogService;
            autocompleteDataLoader.LoadSettings();
            Rates = csvLoader.LoadFixedCurrencyRates(); // TODO make it possible to choose from settings
            VatPercentage = csvLoader.LoadCountryVatRates(); // TODO make it possible to choose from settings
        }

        private void MergeInvoiceItemsToExistingDataPack(Invoice existingInvoice, Invoice aggregatedInvoice)
        {
            var existingInvoiceItems = existingInvoice.InvoiceItems.ToList();
            existingInvoiceItems.AddRange(aggregatedInvoice.InvoiceItems.Where(item => item.Type == InvoiceItemType.Product));

            AggregateItemsOfType(existingInvoiceItems, aggregatedInvoice.InvoiceItems, InvoiceItemType.Discount);
            AggregateItemsOfType(existingInvoiceItems, aggregatedInvoice.InvoiceItems, InvoiceItemType.GiftWrap);
            AggregateItemsOfType(existingInvoiceItems, aggregatedInvoice.InvoiceItems, InvoiceItemType.Shipping);

            existingInvoice.InvoiceItems = existingInvoiceItems;
        }

        private void AggregateItemsOfType(IEnumerable<InvoiceItemBase> items1, IEnumerable<InvoiceItemBase> items2, InvoiceItemType itemType)
        {
            AggregateItems(items1.SingleOrDefault(i => i.Type == itemType), items2.SingleOrDefault(i => i.Type == itemType));
        }

        private void AggregateItems(InvoiceItemBase existingItem, InvoiceItemBase aggregatedItem)
        {
            if (existingItem == null || aggregatedItem == null) return;

            if (existingItem.Type != aggregatedItem.Type) 
                throw new ArgumentException("Cannot aggregate items of different type!");

            existingItem.TotalPrice = AggregatePrice(existingItem.TotalPrice, aggregatedItem.TotalPrice); 
            existingItem.VatPrice = AggregatePrice(existingItem.VatPrice, aggregatedItem.VatPrice);
            existingItem.UnitPrice = AggregatePrice(existingItem.UnitPrice, aggregatedItem.UnitPrice);
        }

        private CommonServices.Currency AggregatePrice(CommonServices.Currency c1, CommonServices.Currency c2) // TODO maybe Currency +operator?
        {
            if (!c1.ForeignCurrencyName.EqualsIgnoreCase(c2.ForeignCurrencyName))
                throw new ArgumentException("Aggregated price has different currency!");

            return new CommonServices.Currency(c1.AmountForeign + c2.AmountForeign, c1.ForeignCurrencyName, Rates);
        }

        public IEnumerable<Invoice> LoadAmazonReports(IEnumerable<string> reportsFileNames, InvoiceConversionContext conversionContext)
        {
            var dataFromAmazonReports = _invoicesXmlManager.LoadAmazonReports(reportsFileNames);
            if (dataFromAmazonReports == null) return Array.Empty<Invoice>();

            return MerginInvoicesOfSameOrders(dataFromAmazonReports, conversionContext);
        }

        private IEnumerable<Invoice> MerginInvoicesOfSameOrders(IEnumerable<Dictionary<string, string>> dataFromAmazonReports,  InvoiceConversionContext conversionContext)
        {
            var source = new List<Invoice>();
            foreach (var report in dataFromAmazonReports)
            {
                var singleAmazonInvoice = ProcessInvoiceLine(report, source.Count + 1, conversionContext);
                var existingDataPack = source.FirstOrDefault(i => i.VariableSymbolFull == singleAmazonInvoice.VariableSymbolFull);
                if (existingDataPack != null)
                {
                    MergeInvoiceItemsToExistingDataPack(existingDataPack, singleAmazonInvoice);
                }
                else
                {
                    source.Add(singleAmazonInvoice);
                }
            }

            return source;
        }


        private Invoice ProcessInvoiceLine(
            IReadOnlyDictionary<string, string> valuesFromAmazon, // this is the line
            int invoiceIndex, 
            InvoiceConversionContext conversionContext)
        {
            string shipCountry = valuesFromAmazon["ship-country"];
            var classification = SetClassification(shipCountry);

            decimal giftWrapPrice = 0;
            string giftWrapPriceName = "gift-wrap-price";
            string giftWrapTax = "gift-wrap-tax";
            if (valuesFromAmazon.ContainsKey(giftWrapPriceName) && valuesFromAmazon.ContainsKey(giftWrapTax))
            {
                giftWrapPrice = GetPrice(valuesFromAmazon[giftWrapPriceName], valuesFromAmazon[giftWrapTax], shipCountry);
            }

            var invoice = new Invoice();

            string headerSymPar = valuesFromAmazon["order-id"];
            uint invoiceNumber = (uint)(conversionContext.ExistingInvoiceNumber + invoiceIndex);
            string userId = "Usr01 (" + invoiceIndex.ToString().PadLeft(3, '0') + ")";
            decimal itemFullPrice = GetPrice(valuesFromAmazon["item-price"], valuesFromAmazon["item-tax"], shipCountry); // price for all items
            decimal shippingPrice = GetPrice(valuesFromAmazon["shipping-price"], valuesFromAmazon["shipping-tax"], shipCountry);
            decimal promotionDiscount = decimal.Parse(valuesFromAmazon["item-promotion-discount"]);
            decimal shipPromotionDiscount = decimal.Parse(valuesFromAmazon["ship-promotion-discount"]);
            string currency = _currencyConverter.Convert(valuesFromAmazon["currency"]);
            decimal priceSum = itemFullPrice + shippingPrice - promotionDiscount - shipPromotionDiscount + giftWrapPrice;
            string salesChannel = valuesFromAmazon["sales-channel"];
            string clientName = FormatClientName(valuesFromAmazon["recipient-name"], headerSymPar);
            string city = FormatCity(valuesFromAmazon["ship-city"], valuesFromAmazon["ship-state"], string.Empty, headerSymPar);
            string fullAddress = FormatFullAddress(valuesFromAmazon["ship-address-1"], valuesFromAmazon["ship-address-2"], valuesFromAmazon["ship-address-3"], headerSymPar);
            string phoneNumber = FormatPhoneNumber(valuesFromAmazon["ship-phone-number"], valuesFromAmazon["buyer-phone-number"], headerSymPar);
            string productName = valuesFromAmazon["product-name"];
            string sku = valuesFromAmazon["sku"];
            string shipping = GetShippingType(shipCountry, sku);
            string stockItemIds = GetItemCodeBySku(sku);

            DateTime today = conversionContext.ConvertToDate;
            DateTime taxDate = CalculateTaxDate(today, classification);
            DateTime dueDate = CalculateDueDate(today);

            invoice.UserId = userId;
            invoice.Number = invoiceNumber;
            invoice.VariableSymbolFull = headerSymPar;
            invoice.VariableSymbolShort = TransactionsReader.GetShortVariableCode(headerSymPar, out _);
            invoice.ShipCountryCode = shipCountry;
            invoice.ConversionDate = today;
            invoice.DateTax = taxDate;
            invoice.DateAccounting = today;
            invoice.DateDue = dueDate;

            invoice.Classification = classification;
            invoice.VatType = GetClassificationVatType(classification);

            invoice.ClientInfo = new PartnerInfo
            {
                Name = clientName,
                Address = new Address
                {
                    City = city,
                    Street = fullAddress,
                    Country = shipCountry,
                    Zip = valuesFromAmazon["ship-postal-code"]
                },
                Contact = new ContactData
                {
                    Email = conversionContext.DefaultEmail,
                    Phone = phoneNumber
                }
            };
            invoice.CustomsDeclaration = GetCustomsDeclarationBySku(sku, classification);
            invoice.RelatedWarehouseName = GetSavedWarehouseBySku(sku);
            invoice.SalesChannel = salesChannel;

            invoice.TotalPrice = new CommonServices.Currency(priceSum, currency, Rates);
            if (!IsNonEuCountryByClassification(invoice.Classification))
            {
                invoice.CountryVat = new Vat(VatPercentage[invoice.ShipCountryCode]);
                invoice.TotalPriceVat = new CommonServices.Currency(Math.Round(invoice.TotalPrice.AmountForeign * invoice.CountryVat.ReversePercentage, 2), currency, Rates);
            }

            if (classification == InvoiceVatClassification.RDzasEU) // EU countries except CZ
            {
                invoice.IsMoss = true;
            }

            var invoiceItems = new List<InvoiceItemBase>();
            var invoiceProduct = new InvoiceProduct(invoice);

            invoiceProduct.WarehouseCode = stockItemIds;
            invoiceProduct.WarehouseName = "Zboží";
            invoiceProduct.AmazonSku = sku;

            var invoiceItemProduct = FillInvoiceItem(invoiceProduct, productName, itemFullPrice, decimal.Parse(valuesFromAmazon["quantity-purchased"]));
            invoiceProduct.PackQuantityMultiplier = 1;
            if (!string.IsNullOrEmpty(invoiceProduct.AmazonSku) &&
                _autocompleteData.PackQuantitySku.ContainsKey(invoiceProduct.AmazonSku))
            {
                invoiceProduct.PackQuantityMultiplier = uint.Parse(_autocompleteData.PackQuantitySku[invoiceProduct.AmazonSku]);
            }
            invoiceItems.Add(invoiceItemProduct);

            var invoiceItemShipping = FillInvoiceItem(new InvoiceItemGeneral(invoice, InvoiceItemType.Shipping), shipping,  shippingPrice, 1);
            invoiceItems.Add(invoiceItemShipping);

            if (promotionDiscount != 0)
            {
                var invoiceItemDiscount = FillInvoiceItem(new InvoiceItemGeneral(invoice, InvoiceItemType.Discount), "Discount", promotionDiscount, 1);
                invoiceItems.Add(invoiceItemDiscount);
            }

            if (giftWrapPrice != 0)
            {
                string giftWrapType = "Gift wrap " + valuesFromAmazon["gift-wrap-type"];
                var invoiceItemGiftWrap = FillInvoiceItem(new InvoiceItemGeneral(invoice, InvoiceItemType.GiftWrap), giftWrapType, promotionDiscount, 1);
                invoiceItems.Add(invoiceItemGiftWrap);
            }

            invoice.InvoiceItems = invoiceItems;
            return invoice;
        }


        private InvoiceItemBase FillInvoiceItem(InvoiceItemBase invoiceItem, string name, decimal price, decimal quantity)
        {
            if (name.Length > MaxItemNameLength)
            {
                name = name.Substring(0, MaxItemNameLength);
            }

            invoiceItem.Name = name; 
            invoiceItem.Quantity = quantity;

            invoiceItem.UnitPrice = new CommonServices.Currency(price / quantity, invoiceItem.ParentInvoice.TotalPrice.ForeignCurrencyName, Rates); // should they be connected?
            if (IsNonEuCountryByClassification(invoiceItem.ParentInvoice.Classification)) 
            {
                invoiceItem.TotalPrice = new CommonServices.Currency(price, invoiceItem.ParentInvoice.TotalPrice.ForeignCurrencyName, Rates);
                invoiceItem.VatPrice = new CommonServices.Currency(0, invoiceItem.ParentInvoice.TotalPrice.ForeignCurrencyName, Rates);
            }
            else
            {
                invoiceItem.TotalPrice = new CommonServices.Currency(price * (1 - invoiceItem.ParentInvoice.CountryVat.ReversePercentage), invoiceItem.ParentInvoice.TotalPrice.ForeignCurrencyName, Rates);
                invoiceItem.VatPrice = new CommonServices.Currency(price * invoiceItem.ParentInvoice.CountryVat.ReversePercentage, invoiceItem.ParentInvoice.TotalPrice.ForeignCurrencyName, Rates);
            }

            if (invoiceItem.ParentInvoice.Classification == InvoiceVatClassification.RDzasEU) // EU countries except CZ
            {
                invoiceItem.PercentVat = invoiceItem.ParentInvoice.CountryVat.Percentage * (decimal)100.0; 
            }

            return invoiceItem;
        }


        private decimal GetPrice(string price, string tax, string country)
        {
            if (string.IsNullOrWhiteSpace(price)) return 0;

            if (country.EqualsIgnoreCase("GB") || country.EqualsIgnoreCase("AU"))
                return decimal.Parse(price) - decimal.Parse(tax);

            return decimal.Parse(price);
        }

        private string GetClassificationVatType(InvoiceVatClassification classification)
        {
            return IsNonEuCountryByClassification(classification) ? "nonSubsume" : null;
        }

        private string GetCustomsDeclarationBySku(string sku, InvoiceVatClassification classification)
        {
            if (IsNonEuCountryByClassification(classification) &&
                _autocompleteData.CustomsDeclarationBySku.ContainsKey(sku))
            {
                return _autocompleteData.CustomsDeclarationBySku[sku];
            }
            return string.Empty;
        }

        private string GetShippingType(string shipCountry, string sku)
        {
            string defaultShippingName = "Shipping";
            if (!_euContries.Contains(shipCountry))
            {
                return defaultShippingName;
            }

            if (!_autocompleteData.ShippingNameBySku.ContainsKey(sku))
            {
                return defaultShippingName;
            }

            return _autocompleteData.ShippingNameBySku[sku];
        }

        public void ProcessInvoices(IEnumerable<Invoice> invoices, string fileName)
        {
            _invoicesXmlManager.SerializeXmlInvoice(fileName, invoices);

            _autocompleteDataLoader.SaveSettings(_autocompleteData);
        }

        private string GetItemCodeBySku(string sku)
        {
            if (!_autocompleteData.PohodaProdCodeBySku.ContainsKey(sku)) return ApplicationConstants.EmptyItemCode;

            return _autocompleteData.PohodaProdCodeBySku[sku];
        }

        private string GetSavedWarehouseBySku(string sku)
        {
            string defaultCentreIds = ApplicationConstants.EmptyItemCode;
            if (!_autocompleteData.ProdWarehouseSectionBySku.ContainsKey(sku)) return defaultCentreIds;

            return _autocompleteData.ProdWarehouseSectionBySku[sku];
        }

        private bool IsNonEuCountryByClassification(InvoiceVatClassification classification)
        {
            return classification == InvoiceVatClassification.UVzbozi; // AFWULL
        }

        private DateTime CalculateTaxDate(DateTime conversionDate, InvoiceVatClassification classification)
        {
            if (IsNonEuCountryByClassification(classification))
                return conversionDate.AddDays(5.0);
            return conversionDate;
        }

        private DateTime CalculateDueDate(DateTime purchaseDate)
        {
            return purchaseDate.AddDays(3.0);
        }

        private InvoiceVatClassification SetClassification(string shipCountry)
        {
            if (shipCountry.Equals("CZ")) return InvoiceVatClassification.UDA5; // UDA5 only CZ
            return _euContries.Contains(shipCountry) ? InvoiceVatClassification.RDzasEU : InvoiceVatClassification.UVzbozi; 
            // UDzasEU - European Union countries, UVzboží - the rest
        }

        private string FormatCity(
            string shipCity,
            string shipState,
            string postalCode,
            string amazonOrderNumber)
        {
            string str = shipCity + ", " + shipState + " " + postalCode;
            string message = $"Nazev mesta/zeme je prilis dlouhy v objednavce C.: {amazonOrderNumber}.";
            str = _dialogService.AskToChangeLongStringIfNeeded(message, str, MaxCityLength);

            return str;
        }

        private string FormatPhoneNumber(string shippingPhone, string buyerPhone, string order)
        {
            string str = shippingPhone;
            if (string.IsNullOrEmpty(str))
                str = buyerPhone;
            return str;
        }

        private string FormatClientName(string name, string amazonOrderNumber)
        {
            string message = $"Jmeno zakazniku je prilis dlouhe v objednavce C.: {amazonOrderNumber}." ;

            name = _dialogService.AskToChangeLongStringIfNeeded(message, name, MaxClientNameLength);

            return name;
        }

        private string FormatFullAddress(
            string address1,
            string address2,
            string address3,
            string amazonOrderNumber)
        {
            string str = address1;
            if (!string.IsNullOrEmpty(address2))
                str = str + ", " + address2;
            if (!string.IsNullOrEmpty(address3))
                str = str + ", " + address3;

            string message = $"Addressa je prilis dlouha v objednavce C.: {amazonOrderNumber}.";
            str = _dialogService.AskToChangeLongStringIfNeeded(message, str, MaxAddressNameLength);
            return str;
        }
    }
}
