using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Shmap.BusinessLogic.Currency;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Mapp;
using Shmap.BusinessLogic.Transactions;

namespace Shmap.BusinessLogic.Invoices
{
    public class InvoiceConverter : IInteractionRequester
    {
        public EventHandler<string> UserNotification { get; init; }
        public EventHandler<string> UserInteraction { get; init; }

        private readonly IAutocompleteData _autocompleteData;
        private readonly CurrencyConverter _currencyConverter;
        private readonly IInvoicesXmlManager _invoicesXmlManager;
        private readonly Func<string, string, int, string> _askUserToChangeString;
        private readonly AutocompleteDataLoader _autocompleteDataLoader;
        private static readonly int MaxItemNameLength = 85;
        private readonly int MaxAddressNameLength = 60;
        private readonly int MaxCityLength = 45;
        private readonly int MaxClientNameLength = 30;
        private IEnumerable<InvoiceXml.dataPackDataPackItem> _convertedInvoices;
        private Dictionary<string, decimal> Rates;
        private Dictionary<string, decimal> VatRates;

        private string[] DefaultShippingNames = { "Shipping", "Registered shipping" };
        public string[] _euContries = // TODO from settings
        {
            "BE", "BG", "DK", "EE", "FI", "FR", "IE", "IT", "CY", "LT", "LV", "LU", "HU", "HR", "MT", "DE",
            "NL", "PL", "PT", "AT", "RO", "GR", "SK", "SI", "ES", "SE", "EU", "CZ"
        };


        public uint ExistingInvoiceNumber { get; set; }
        public decimal CountryVat { get; set; }
        public string DefaultEmail { get; set; }
        public string LatestTrackingCode { get; set; }

        public ObservableCollection<InvoiceXml.dataPackDataPackItem> InvoicesTable { get; set; } = new();

        public ObservableCollection<InvoiceItemWithDetails> InvoiceItemsAll { get; set; } = new();

        public InvoiceConverter(IAutocompleteData autocompleteData, CurrencyConverter currencyConverter,
            CsvLoader csvLoader, IInvoicesXmlManager invoicesXmlManager,
            Func<string, string, int, string> askUserToChangeString, AutocompleteDataLoader autocompleteDataLoader)
        {
            _autocompleteData = autocompleteData;
            this._currencyConverter = currencyConverter;
            _invoicesXmlManager = invoicesXmlManager;
            _askUserToChangeString = askUserToChangeString; // Awfull dirty hack for now
            _autocompleteDataLoader = autocompleteDataLoader;
            Rates = csvLoader.LoadFixedCurrencyRates(); // TODO make it possible to choose from settings
            VatRates = csvLoader.LoadCountryVatRates(); // TODO make it possible to choose from settings
        }

        private void MergeInvoiceItemsToExistingDataPack(
          InvoiceXml.dataPackDataPackItem existingDataPack,
          InvoiceXml.dataPackDataPackItem dataPackDataPackItem)
        {
            var list = (existingDataPack.invoice.invoiceDetail).ToList();
            var invoiceInvoiceItem = (dataPackDataPackItem.invoice.invoiceDetail).First();
            list.Insert(list.Count - 1, invoiceInvoiceItem);

            var shippingItem1 = GetShippingItem(list); // summarizing shipping form (multiple) items to single Invoice
            var shippingItem2 = GetShippingItem(dataPackDataPackItem.invoice.invoiceDetail);
            shippingItem1.foreignCurrency.unitPrice += shippingItem2.foreignCurrency.unitPrice;
            shippingItem1.foreignCurrency.price += shippingItem2.foreignCurrency.price;
            shippingItem1.foreignCurrency.priceSum += shippingItem2.foreignCurrency.priceSum;
            shippingItem1.foreignCurrency.priceVAT += shippingItem2.foreignCurrency.priceVAT;
            shippingItem1.homeCurrency.unitPrice += shippingItem2.homeCurrency.unitPrice;
            shippingItem1.homeCurrency.price += shippingItem2.homeCurrency.price;
            shippingItem1.homeCurrency.priceSum += shippingItem2.homeCurrency.priceSum;
            shippingItem1.homeCurrency.priceVAT += shippingItem2.homeCurrency.priceVAT;

            // same crazy stuff with discount - find a new and copy discount
            var discountItem1 = GetDiscountItem(list); // summarizing discounts form (multiple) items to single Invoice
            var discountItem2 = GetDiscountItem(dataPackDataPackItem.invoice.invoiceDetail);
            if (discountItem1 != null && discountItem2 != null)
            {
                discountItem1.foreignCurrency.unitPrice += discountItem2.foreignCurrency.unitPrice;
                discountItem1.foreignCurrency.price += discountItem2.foreignCurrency.price;
                discountItem1.foreignCurrency.priceSum += discountItem2.foreignCurrency.priceSum;
                discountItem1.foreignCurrency.priceVAT += discountItem2.foreignCurrency.priceVAT;
                discountItem1.homeCurrency.unitPrice += discountItem2.homeCurrency.unitPrice;
                discountItem1.homeCurrency.price += discountItem2.homeCurrency.price;
                discountItem1.homeCurrency.priceSum += discountItem2.homeCurrency.priceSum;
                discountItem1.homeCurrency.priceVAT += discountItem2.homeCurrency.priceVAT;
            }

            existingDataPack.invoice.invoiceDetail = list.ToArray();
        }

        private static InvoiceXml.invoiceInvoiceItem GetDiscountItem(
            IEnumerable<InvoiceXml.invoiceInvoiceItem> existingItems)
        {
            return existingItems.SingleOrDefault(item => item.text.EqualsIgnoreCase("discount"));
        }

        private static InvoiceXml.invoiceInvoiceItem GetShippingItem(
          IEnumerable<InvoiceXml.invoiceInvoiceItem> existingItems)
        {
            return existingItems.Single(item => item.IsShipping);
        }

        public void LoadAmazonReports(IEnumerable<string> reportsFileNames, DateTime toDate)
        {
            var dataFromAmazonReports = _invoicesXmlManager.LoadAmazonReports(reportsFileNames);

            if (dataFromAmazonReports == null) return;

            var source = MerginInvoicesOfSameOrders(dataFromAmazonReports, toDate);

            var invoiceInvoiceItems = source.SelectMany(di => di.invoice.invoiceDetail);
            InvoiceItemsAll.Clear();
            foreach (var invoiceItem in invoiceInvoiceItems)
            {
                var itemWithDetails = new InvoiceItemWithDetails(invoiceItem, source.Single(di => (di.invoice.invoiceDetail).Contains(invoiceItem)).invoice.invoiceHeader);
                if (!itemWithDetails.Item.IsShipping && itemWithDetails.Item.amazonSkuCode != null && _autocompleteData.PackQuantitySku.ContainsKey(itemWithDetails.Item.amazonSkuCode))
                // TODO SAME STUFF AS in TopDataGrid_CellEditEnding -> Item.amazonSkuCode, should be abstracted !!! otherwise with each change it will be required to find all those places
                {
                    itemWithDetails.PackQuantityMultiplier = int.Parse(_autocompleteData.PackQuantitySku[itemWithDetails.Item.amazonSkuCode]);
                }
                InvoiceItemsAll.Add(itemWithDetails);
            }

            InvoicesTable.Clear();
            foreach (var dataPackItem in source)
            {
                InvoicesTable.Add(dataPackItem);
            }
            _convertedInvoices = source;
        }

        private List<InvoiceXml.dataPackDataPackItem> MerginInvoicesOfSameOrders(IEnumerable<Dictionary<string, string>> dataFromAmazonReports, DateTime toDate)
        {
            var source = new List<InvoiceXml.dataPackDataPackItem>();
            foreach (var report in dataFromAmazonReports)
            {
                var singleAmazonInvoice = FillDataPackItem(report, source.Count + 1, toDate);
                var existingDataPack = source.FirstOrDefault(di =>
                    di.invoice.invoiceHeader.symVar == singleAmazonInvoice.invoice.invoiceHeader.symVar);
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


        private InvoiceXml.dataPackDataPackItem FillDataPackItem(Dictionary<string, string> valuesFromAmazon, int index, DateTime toDate)
        {
            string invoiceType = "issuedInvoice";
            string accountingIds = "3Fv";

            string shipCountry = valuesFromAmazon["ship-country"];
            string classification = SetClassification(shipCountry);

            if (!IsNonEuCountryByClassification(classification))
            {
                CountryVat = GetVatByCountry(shipCountry);
            }

            string headerSymPar = valuesFromAmazon["order-id"];
            uint invoiceNumber = (uint)(ExistingInvoiceNumber + index);
            var packDataPackItem = _invoicesXmlManager.PrepareDatapackItem();
            string userId = "Usr01 (" + index.ToString().PadLeft(3, '0') + ")";
            decimal itemPrice = GetPrice(valuesFromAmazon["item-price"], valuesFromAmazon["item-tax"], classification);
            decimal shippingPrice = GetPrice(valuesFromAmazon["shipping-price"], valuesFromAmazon["shipping-tax"], classification);
            decimal promotionDiscount = decimal.Parse(valuesFromAmazon["item-promotion-discount"]);
            decimal shipPromotionDiscount = decimal.Parse(valuesFromAmazon["ship-promotion-discount"]);
            string currency = _currencyConverter.Convert(valuesFromAmazon["currency"]);
            decimal currencyRate = GetCurrencyRate(currency);
            decimal priceSum = itemPrice + shippingPrice - promotionDiscount - shipPromotionDiscount;
            decimal currencyPriceHighSum = priceSum * currencyRate;
            string salesChannel = valuesFromAmazon["sales-channel"];
            string clientName = FormatClientName(valuesFromAmazon["recipient-name"], headerSymPar);
            string city = FormatCity(valuesFromAmazon["ship-city"], valuesFromAmazon["ship-state"], string.Empty, headerSymPar);
            string fullAddress = FormatFullAddress(valuesFromAmazon["ship-address-1"], valuesFromAmazon["ship-address-2"], valuesFromAmazon["ship-address-3"], headerSymPar);
            string phoneNumber = FormatPhoneNumber(valuesFromAmazon["ship-phone-number"], valuesFromAmazon["buyer-phone-number"], headerSymPar);

            string productName = valuesFromAmazon["product-name"];
            string sku = valuesFromAmazon["sku"];
            string shipping = GetShippingType(shipCountry, sku);

            string stockItemIds = GetItemCodeBySku(sku);

            DateTime today = toDate;
            DateTime taxDate = CalculateTaxDate(today, classification);
            DateTime dueDate = CalculateDueDate(today);
            packDataPackItem.id = userId;
            packDataPackItem.invoice.invoiceHeader.invoiceType = invoiceType;
            packDataPackItem.invoice.invoiceHeader.number.numberRequested = invoiceNumber;
            packDataPackItem.invoice.invoiceHeader.symVar = TransactionsReader.GetShortVariableCode(headerSymPar, out _);
            packDataPackItem.invoice.invoiceHeader.symPar = headerSymPar;
            packDataPackItem.invoice.invoiceHeader.date = today;
            packDataPackItem.invoice.invoiceHeader.dateTax = taxDate;
            packDataPackItem.invoice.invoiceHeader.dateAccounting = today;
            packDataPackItem.invoice.invoiceHeader.dateDue = dueDate;
            packDataPackItem.invoice.invoiceHeader.accounting.ids = accountingIds;
            packDataPackItem.invoice.invoiceHeader.classificationVAT.ids = classification;
            packDataPackItem.invoice.invoiceHeader.classificationVAT.classificationVATType = GetClassificationVatType(classification);
            packDataPackItem.invoice.invoiceHeader.text = "This is your invoice:";
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.name = clientName;
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.city = city; // 45 symbols max
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.street = fullAddress;
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.country.ids = shipCountry;
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.zip = valuesFromAmazon["ship-postal-code"];
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.phone = phoneNumber;
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.mobilPhone = GetCustomsDeclarationBySku(sku, classification);
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.email = DefaultEmail;
            packDataPackItem.invoice.invoiceHeader.paymentType.ids = salesChannel;
            packDataPackItem.invoice.invoiceHeader.centre.ids = GetSavedCenterBySku(sku);
            packDataPackItem.invoice.invoiceHeader.liquidation.amountHome = priceSum * currencyRate;
            packDataPackItem.invoice.invoiceHeader.liquidation.amountForeign = priceSum;
            packDataPackItem.invoice.invoiceHeader.histRate = true; // always true


            if (classification.EqualsIgnoreCase("UDzasEU")) // EU countries except CZ
            {
                packDataPackItem.invoice.invoiceHeader.MOSS = new InvoiceXml.invoiceInvoiceHeaderMOSS() { ids = shipCountry };
                packDataPackItem.invoice.invoiceHeader.evidentiaryResourcesMOSS =
                    new InvoiceXml.invoiceInvoiceHeaderEvidentiaryResourcesMOSS() { ids = "A" };
            }

            var invoiceInvoiceItemList = new List<InvoiceXml.invoiceInvoiceItem>();

            decimal quantity = decimal.Parse(valuesFromAmazon["quantity-purchased"]);
            var invoiceItemProduct = CreateInvoiceItem(productName, classification, itemPrice / quantity, currencyRate, quantity);
            invoiceItemProduct.code = stockItemIds;
            invoiceItemProduct.stockItem = new InvoiceXml.invoiceInvoiceItemStockItem();
            invoiceItemProduct.stockItem.stockItem = new InvoiceXml.stockItem();
            invoiceItemProduct.stockItem.stockItem.ids = stockItemIds;
            invoiceItemProduct.stockItem.store = new InvoiceXml.store();
            invoiceItemProduct.stockItem.store.ids = "Zboží";
            invoiceItemProduct.amazonSkuCode = sku;
            invoiceInvoiceItemList.Add(invoiceItemProduct);

            var invoiceItemShipping = CreateInvoiceItem(shipping, classification, shippingPrice, currencyRate, 1);
            invoiceItemShipping.IsShipping = true;
            invoiceInvoiceItemList.Add(invoiceItemShipping);

            if (promotionDiscount != 0)
            {
                var invoiceItemDiscount = CreateInvoiceItem("Discount", classification, promotionDiscount, currencyRate, 1);
                invoiceInvoiceItemList.Add(invoiceItemDiscount);
            }

            packDataPackItem.invoice.invoiceDetail = invoiceInvoiceItemList.ToArray();
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceNone = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceLow = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceLowVAT = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceLowSum = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHigh = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighVAT = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighSum = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.price3 = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.price3VAT = 0;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.price3Sum = 0;
            if (IsNonEuCountryByClassification(classification)) // AWFUL! !
            {
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceNone = priceSum * currencyRate;
            }
            else
            {
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHigh = currencyPriceHighSum - Math.Round(currencyPriceHighSum * CountryVat, 2);
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighVAT = Math.Round(currencyPriceHighSum * CountryVat, 2);
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighSum = currencyPriceHighSum;
            }
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.amount = 1;
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.currency.ids = currency;
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.rate = currencyRate;
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.priceSum = priceSum;
            return packDataPackItem;
        }

        private InvoiceXml.invoiceInvoiceItem CreateInvoiceItem(string name, string classification, decimal price,
            decimal currencyRate, decimal quantity)
        {
            var invoiceItemShipping = new InvoiceXml.invoiceInvoiceItem();
            invoiceItemShipping.text = name;
            invoiceItemShipping.quantity = quantity;
            invoiceItemShipping.payVAT = true;
            invoiceItemShipping.discountPercentage = 0;
            invoiceItemShipping.foreignCurrency = new InvoiceXml.invoiceInvoiceItemForeignCurrency();
            invoiceItemShipping.homeCurrency = new InvoiceXml.invoiceInvoiceItemHomeCurrency();
            invoiceItemShipping.PDP = false;

            if (IsNonEuCountryByClassification(classification)) // TODO CHECK if quantity here is correct
            {
                invoiceItemShipping.rateVAT = "historyHigh";
                invoiceItemShipping.foreignCurrency.unitPrice = price / quantity;
                invoiceItemShipping.foreignCurrency.price = price;
                invoiceItemShipping.foreignCurrency.priceSum = price;
                invoiceItemShipping.foreignCurrency.priceVAT = 0;

                invoiceItemShipping.homeCurrency.unitPrice = invoiceItemShipping.foreignCurrency.unitPrice * currencyRate;
                invoiceItemShipping.homeCurrency.price = invoiceItemShipping.foreignCurrency.price * currencyRate;
                invoiceItemShipping.homeCurrency.priceSum = invoiceItemShipping.foreignCurrency.priceSum * currencyRate;
                invoiceItemShipping.homeCurrency.priceVAT = 0;
            }
            else
            {
                invoiceItemShipping.rateVAT = "historyHigh";
                invoiceItemShipping.foreignCurrency.unitPrice = price / quantity;
                invoiceItemShipping.foreignCurrency.price = price * (1 - CountryVat); 
                invoiceItemShipping.foreignCurrency.priceSum = invoiceItemShipping.foreignCurrency.price;
                invoiceItemShipping.foreignCurrency.priceVAT = price * CountryVat;
                
                invoiceItemShipping.homeCurrency.unitPrice = invoiceItemShipping.foreignCurrency.unitPrice * currencyRate;
                invoiceItemShipping.homeCurrency.price = invoiceItemShipping.foreignCurrency.price * currencyRate;  
                invoiceItemShipping.homeCurrency.priceSum = invoiceItemShipping.foreignCurrency.priceSum * currencyRate;
                invoiceItemShipping.homeCurrency.priceVAT = invoiceItemShipping.foreignCurrency.priceVAT * currencyRate;
            }

            return invoiceItemShipping;
        }

        private decimal GetVatByCountry(string shipCountry)
        {
            try
            {
                var vat = VatRates[shipCountry]; // TODO make VAT special struct - for recalculations
                return VatRates[shipCountry] / (1 + vat);
            }
            catch
            {
                throw new ArgumentException($"Could not find VAT for country {shipCountry}!");
            }
        }

        private decimal GetPrice(string price, string tax, string classification)
        {
            if (classification.EqualsIgnoreCase("GB") || classification.EqualsIgnoreCase("AU"))
                return decimal.Parse(price) + decimal.Parse(tax);

            return decimal.Parse(price);
        }

        private string GetClassificationVatType(string classification)
        {
            return IsNonEuCountryByClassification(classification) ? "nonSubsume" : null;
        }

        private string GetCustomsDeclarationBySku(string sku, string classification)
        {
            if (IsNonEuCountryByClassification(classification) && _autocompleteData.CustomsDeclarationBySku.ContainsKey(sku))
            { return _autocompleteData.CustomsDeclarationBySku[sku]; }
            return string.Empty;
        }

        private string GetShippingType(string shipCountry, string sku)
        {
            string defaultShippingName = DefaultShippingNames.First();
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

        public void ProcessInvoices(string fileName)
        {
            var invoice = _invoicesXmlManager.PrepareInvoice(_convertedInvoices);

            FixItems(InvoiceItemsAll);
            if (_convertedInvoices == null || !_convertedInvoices.Any())
            {
                UserNotification.Invoke(this, "Zadne faktury nebyly konvertovany!"); // TODO solve using OperationResult
                return;
            }

            _invoicesXmlManager.SerializeXmlInvoice(fileName, invoice);
            UpdateExistingInvoiceNumber(invoice);

            _autocompleteDataLoader.SaveSettings(_autocompleteData);
        }

        private string GetItemCodeBySku(string sku)
        {
            if (!_autocompleteData.PohodaProdCodeBySku.ContainsKey(sku)) return ApplicationConstants.EmptyItemCode;

            return _autocompleteData.PohodaProdCodeBySku[sku];
        }

        private string GetSavedCenterBySku(string sku)
        {
            string defaultCentreIds = ApplicationConstants.EmptyItemCode;
            if (!_autocompleteData.ProdWarehouseSectionBySku.ContainsKey(sku)) return defaultCentreIds;

            return _autocompleteData.ProdWarehouseSectionBySku[sku];
        }

        private void UpdateExistingInvoiceNumber(InvoiceXml.dataPack invoice)
        {
            ExistingInvoiceNumber += (uint)(invoice.dataPackItem).Count();
        }

        private bool IsNonEuCountryByClassification(string countryCode)
        {
            return countryCode.EqualsIgnoreCase("UVzboží"); // AFWULL
        }

        private DateTime CalculateTaxDate(DateTime conversionDate, string classification)
        {
            if (IsNonEuCountryByClassification(classification))
                return conversionDate.AddDays(5.0);
            return conversionDate;
        }

        private DateTime CalculateDueDate(DateTime purchaseDate)
        {
            return purchaseDate.AddDays(3.0);
        }

        private string SetClassification(string shipCountry)
        {
            if (shipCountry.Equals("CZ")) return "UDA5"; // UDA5 only CZ
            return (_euContries).Contains(shipCountry) ? "UDzasEU" : "UVzboží"; // UDzasEU - European Union countries, UVzboží - the rest
        }

        private void FixItems(IEnumerable<InvoiceItemWithDetails> invoiceItemsAll)
        {
            foreach (var itemWithDetailes in invoiceItemsAll)
            {
                if (itemWithDetailes.Item.stockItem != null)
                    itemWithDetailes.Item.stockItem.stockItem.ids = itemWithDetailes.Item.code;
                if (itemWithDetailes.Item.text.Length > MaxItemNameLength)
                    itemWithDetailes.Item.text = itemWithDetailes.Item.text.Substring(0, MaxItemNameLength);
            }
        }

        private decimal GetCurrencyRate(string currency)
        {
            return Rates.Single(r => r.Key.Equals(currency, StringComparison.InvariantCultureIgnoreCase)).Value;
        }


        private string FormatCity(
            string shipCity,
            string shipState,
            string postalCode,
            string amazonOrderNumber)
        {
            string str = shipCity + ", " + shipState + " " + postalCode;

            string message = "Nazev mesta/zeme je prilis dlouhy v objednavce C.: "
                             + amazonOrderNumber;

            str = _askUserToChangeString(message, str, MaxCityLength);

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
            string message = "Jmeno zakazniku je prilis dlouhe v objednavce C.: " + amazonOrderNumber;

            name = _askUserToChangeString(message, name, MaxClientNameLength);

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

            string message = "Addressa je prilis dlouha v objednavce C.: "
                             + amazonOrderNumber;
            str = _askUserToChangeString(message, str, MaxAddressNameLength);
            return str;
        }
    }
}
