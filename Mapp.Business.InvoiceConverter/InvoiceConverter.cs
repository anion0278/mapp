using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shmap.BusinessLogic.Currency;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Mapp;

namespace Shmap.BusinessLogic.Invoices
{
    public class InvoiceConverter: IInteractionRequester
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
        private IEnumerable<InvoiceXML.dataPackDataPackItem> _convertedInvoices;
        private Dictionary<string, decimal> Rates;

        private string[] DefaultShippingNames = { "Shipping", "Registered shipping" };
        public string[] _euContries = // TODO from settings
        {
            "BE", "BG", "CZ", "DK", "EE", "FI", "FR", "IE", "IT", "CY", "LT", "LV", "LU", "HU", "HR", "MT", "DE",
            "NL", "PL", "PT", "AT", "RO", "GR", "SK", "SI", "ES", "SE", "GB", "EU"
        };


        public uint ExistingInvoiceNumber { get; set; }
        public decimal DPH { get; set; }
        public string DefaultEmail { get; set; }
        public string LatestTrackingCode { get; set; }
        
        public ObservableCollection<InvoiceXML.dataPackDataPackItem> InvoicesTable { get; set; } = new();

        public ObservableCollection<InvoiceItemWithDetails> InvoiceItemsAll { get; set; } = new();

        public InvoiceConverter(IAutocompleteData autocompleteData, CurrencyConverter currencyConverter,
            CurrencyLoader currencyLoader, IInvoicesXmlManager invoicesXmlManager,
            Func<string, string, int, string> askUserToChangeString, AutocompleteDataLoader autocompleteDataLoader)
        {
            _autocompleteData = autocompleteData;
            this._currencyConverter = currencyConverter;
            _invoicesXmlManager = invoicesXmlManager;
            _askUserToChangeString = askUserToChangeString; // Awfull dirty hack for now
            _autocompleteDataLoader = autocompleteDataLoader;
            Rates = currencyLoader.LoadFixedCurrencyRates(); // TODO make it possible to choose from settings
        }

        private void MergeInvoiceItemsToExistingDataPack(
          InvoiceXML.dataPackDataPackItem existingDataPack,
          InvoiceXML.dataPackDataPackItem dataPackDataPackItem)
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

        private static InvoiceXML.invoiceInvoiceItem GetDiscountItem(
            IEnumerable<InvoiceXML.invoiceInvoiceItem> existingItems)
        {
            return existingItems.SingleOrDefault(item => item.text.EqualsIgnoreCase("discount"));
        }

        private static InvoiceXML.invoiceInvoiceItem GetShippingItem(
          IEnumerable<InvoiceXML.invoiceInvoiceItem> existingItems)
        {
            return existingItems.Single(item => item.IsShipping);
        }

        public void LoadAmazonReports(IEnumerable<string> reportsFileNames)
        {
            var dataFromAmazonReports = _invoicesXmlManager.LoadAmazonReports(reportsFileNames);

            if (dataFromAmazonReports == null) return;

            var source = MerginInvoicesOfSameOrders(dataFromAmazonReports);

            var invoiceInvoiceItems = source.SelectMany(di => di.invoice.invoiceDetail);
            InvoiceItemsAll.Clear();
            foreach (var invoiceItem in invoiceInvoiceItems)
            {
                var itemWithDetails = new InvoiceItemWithDetails(invoiceItem, source.Single(di => (di.invoice.invoiceDetail).Contains(invoiceItem)).invoice.invoiceHeader);
                if (!itemWithDetails.Item.IsShipping && itemWithDetails.Item.amazonSkuCode != null && _autocompleteData.PackQuantityByItemName.ContainsKey(itemWithDetails.Item.amazonSkuCode)) 
                    // TODO SAME STUFF AS in TopDataGrid_CellEditEnding -> Item.amazonSkuCode, should be abstracted !!! otherwise with each change it will be required to find all those places
                {
                    itemWithDetails.PackQuantityMultiplier = int.Parse(_autocompleteData.PackQuantityByItemName[itemWithDetails.Item.amazonSkuCode]);
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

        private List<InvoiceXML.dataPackDataPackItem> MerginInvoicesOfSameOrders(IEnumerable<Dictionary<string, string>> dataFromAmazonReports)
        {
            var source = new List<InvoiceXML.dataPackDataPackItem>();
            foreach (var report in dataFromAmazonReports)
            {
                var singleAmazonInvoice = FillDataPackItem(report, source.Count + 1);
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


        private InvoiceXML.dataPackDataPackItem FillDataPackItem(Dictionary<string, string> valuesFromAmazon, int index)
        {
            string invoiceType = "issuedInvoice";
            string accountingIds = "3Fv";

            string headerSymPar = valuesFromAmazon["order-id"];
            uint invoiceNumber = (uint)(ExistingInvoiceNumber + index);
            var packDataPackItem = _invoicesXmlManager.PrepareDatapackItem();
            string userId = "Usr01 (" + index.ToString().PadLeft(3, '0') + ")";
            decimal itemPrice = decimal.Parse(valuesFromAmazon["item-price"]);
            decimal shippingPrice = decimal.Parse(valuesFromAmazon["shipping-price"]);
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
            string shipCountry = valuesFromAmazon["ship-country"];
            string classification = SetClassification(shipCountry);
            string productName = FormatNameOfItem(valuesFromAmazon["product-name"]);
            string sku = valuesFromAmazon["sku"];
            string shipping = GetShippingType(shipCountry, sku);

            string stockItemIds = GetItemCodeBySku(sku);

            DateTime today = DateTime.Today;
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
            packDataPackItem.invoice.invoiceHeader.classificationVAT.classificationVATType = classification == "UVzboží" ? "nonSubsume" : (string)null;
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
            var invoiceInvoiceItemList = new List<InvoiceXML.invoiceInvoiceItem>();
            var invoiceItem = new InvoiceXML.invoiceInvoiceItem();
            decimal quantity = decimal.Parse(valuesFromAmazon["quantity-purchased"]);
            invoiceItem.quantity = quantity;
            invoiceItem.payVAT = true;
            invoiceItem.discountPercentage = 0;
            invoiceItem.foreignCurrency = new InvoiceXML.invoiceInvoiceItemForeignCurrency();
            invoiceItem.homeCurrency = new InvoiceXML.invoiceInvoiceItemHomeCurrency();
            invoiceItem.stockItem = new InvoiceXML.invoiceInvoiceItemStockItem();
            invoiceItem.stockItem.stockItem = new InvoiceXML.stockItem();
            invoiceItem.stockItem.store = new InvoiceXML.store();
            invoiceItem.amazonSkuCode = sku;

            if (classification == "UVzboží")
            {
                invoiceItem.rateVAT = "none";
                invoiceItem.foreignCurrency.unitPrice = itemPrice / quantity;
                invoiceItem.foreignCurrency.price = invoiceItem.foreignCurrency.unitPrice * quantity;
                invoiceItem.foreignCurrency.priceSum = invoiceItem.foreignCurrency.price;
                invoiceItem.foreignCurrency.priceVAT = 0;
                invoiceItem.homeCurrency.unitPrice = invoiceItem.foreignCurrency.unitPrice * currencyRate;
                invoiceItem.homeCurrency.price = invoiceItem.foreignCurrency.price * currencyRate;
                invoiceItem.homeCurrency.priceSum *= currencyRate;
                invoiceItem.homeCurrency.priceVAT = 0;
            }
            else
            {
                invoiceItem.rateVAT = "high";
                invoiceItem.foreignCurrency.unitPrice = itemPrice / quantity;
                invoiceItem.foreignCurrency.price = invoiceItem.foreignCurrency.unitPrice * quantity * (1 - DPH);
                invoiceItem.foreignCurrency.priceVAT = invoiceItem.foreignCurrency.unitPrice * quantity * DPH;
                invoiceItem.foreignCurrency.priceSum = invoiceItem.foreignCurrency.unitPrice * quantity;
                invoiceItem.homeCurrency.unitPrice = invoiceItem.foreignCurrency.unitPrice * currencyRate;
                invoiceItem.homeCurrency.price = invoiceItem.foreignCurrency.price * currencyRate;
                invoiceItem.homeCurrency.priceVAT = invoiceItem.foreignCurrency.priceVAT * currencyRate;
                invoiceItem.homeCurrency.priceSum = invoiceItem.foreignCurrency.priceSum * currencyRate;
            }
            invoiceItem.code = stockItemIds;
            invoiceItem.text = productName;
            invoiceItem.stockItem.store.ids = "Zboží";
            invoiceItem.stockItem.stockItem.ids = stockItemIds;
            invoiceItem.PDP = false;
            var invoiceItemShipping = new InvoiceXML.invoiceInvoiceItem();
            invoiceItemShipping.text = shipping;
            invoiceItemShipping.IsShipping = true;
            invoiceItemShipping.quantity = 1;
            invoiceItemShipping.payVAT = true;
            invoiceItemShipping.discountPercentage = 0;
            invoiceItemShipping.foreignCurrency = new InvoiceXML.invoiceInvoiceItemForeignCurrency();
            invoiceItemShipping.homeCurrency = new InvoiceXML.invoiceInvoiceItemHomeCurrency();
            if (classification == "UVzboží")
            {
                invoiceItemShipping.rateVAT = "none";
                invoiceItemShipping.foreignCurrency.unitPrice = shippingPrice;
                invoiceItemShipping.foreignCurrency.price = invoiceItemShipping.foreignCurrency.unitPrice;
                invoiceItemShipping.foreignCurrency.priceSum = invoiceItemShipping.foreignCurrency.price;
                invoiceItemShipping.foreignCurrency.priceVAT = 0;
                invoiceItemShipping.homeCurrency.unitPrice = invoiceItemShipping.foreignCurrency.unitPrice * currencyRate;
                invoiceItemShipping.homeCurrency.price = invoiceItemShipping.foreignCurrency.price * currencyRate;
                invoiceItemShipping.homeCurrency.priceSum *= currencyRate;
                invoiceItemShipping.homeCurrency.priceVAT = 0;
            }
            else
            {
                invoiceItemShipping.rateVAT = "high";
                invoiceItemShipping.foreignCurrency.unitPrice = shippingPrice;
                invoiceItemShipping.foreignCurrency.price = invoiceItemShipping.foreignCurrency.unitPrice * quantity * (1 - DPH);
                invoiceItemShipping.foreignCurrency.priceVAT = invoiceItemShipping.foreignCurrency.unitPrice * quantity * DPH;
                invoiceItemShipping.foreignCurrency.priceSum = invoiceItemShipping.foreignCurrency.unitPrice * quantity;
                invoiceItemShipping.homeCurrency.unitPrice = invoiceItemShipping.foreignCurrency.unitPrice * currencyRate;
                invoiceItemShipping.homeCurrency.price = invoiceItemShipping.foreignCurrency.price * currencyRate;
                invoiceItemShipping.homeCurrency.priceVAT = invoiceItemShipping.foreignCurrency.priceVAT * currencyRate;
                invoiceItemShipping.homeCurrency.priceSum = invoiceItemShipping.foreignCurrency.priceSum * currencyRate;
            }
            invoiceItemShipping.PDP = false;
            invoiceInvoiceItemList.Add(invoiceItem);
            invoiceInvoiceItemList.Add(invoiceItemShipping);
            if (promotionDiscount != 0)
            {
                var invoiceItemDiscount = new InvoiceXML.invoiceInvoiceItem();
                invoiceItemDiscount.text = "Discount";
                invoiceItemDiscount.quantity = 1;
                invoiceItemDiscount.payVAT = true;
                invoiceItemDiscount.discountPercentage = 0;
                invoiceItemDiscount.foreignCurrency = new InvoiceXML.invoiceInvoiceItemForeignCurrency();
                invoiceItemDiscount.homeCurrency = new InvoiceXML.invoiceInvoiceItemHomeCurrency();
                if (classification == "UVzboží")
                {
                    invoiceItemDiscount.rateVAT = "none";
                    invoiceItemDiscount.foreignCurrency.unitPrice = promotionDiscount;
                    invoiceItemDiscount.foreignCurrency.price = invoiceItemDiscount.foreignCurrency.unitPrice;
                    invoiceItemDiscount.foreignCurrency.priceSum = invoiceItemDiscount.foreignCurrency.price;
                    invoiceItemDiscount.foreignCurrency.priceVAT = 0;
                    invoiceItemDiscount.homeCurrency.unitPrice = invoiceItemDiscount.foreignCurrency.unitPrice * currencyRate;
                    invoiceItemDiscount.homeCurrency.price = invoiceItemDiscount.foreignCurrency.price * currencyRate;
                    invoiceItemDiscount.homeCurrency.priceSum *= currencyRate;
                    invoiceItemDiscount.homeCurrency.priceVAT = 0;
                }
                else
                {
                    invoiceItemDiscount.rateVAT = "high";
                    invoiceItemDiscount.foreignCurrency.unitPrice = promotionDiscount;
                    invoiceItemDiscount.foreignCurrency.price = invoiceItemDiscount.foreignCurrency.unitPrice * quantity * (1 - DPH);
                    invoiceItemDiscount.foreignCurrency.priceVAT = invoiceItemDiscount.foreignCurrency.unitPrice * quantity * DPH;
                    invoiceItemDiscount.foreignCurrency.priceSum = invoiceItemDiscount.foreignCurrency.unitPrice * quantity;
                    invoiceItemDiscount.homeCurrency.unitPrice = invoiceItemDiscount.foreignCurrency.unitPrice * currencyRate;
                    invoiceItemDiscount.homeCurrency.price = invoiceItemDiscount.foreignCurrency.price * currencyRate;
                    invoiceItemDiscount.homeCurrency.priceVAT = invoiceItemDiscount.foreignCurrency.priceVAT * currencyRate;
                    invoiceItemDiscount.homeCurrency.priceSum = invoiceItemDiscount.foreignCurrency.priceSum * currencyRate;
                }
                invoiceItemDiscount.PDP = false;
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
            if (classification == "UVzboží") // AWFUL! !
            {
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceNone = priceSum * currencyRate;
            }
            else
            {
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHigh = currencyPriceHighSum - Math.Round(currencyPriceHighSum * DPH, 2);
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighVAT = Math.Round(currencyPriceHighSum * DPH, 2);
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighSum = currencyPriceHighSum;
            }
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.amount = 1;
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.currency.ids = currency;
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.rate = currencyRate;
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.priceSum = priceSum;
            return packDataPackItem;
        }

        private string GetCustomsDeclarationBySku(string sku, string classification)
        {
            if (classification.EqualsIgnoreCase("UVzboží") && _autocompleteData.CustomsDeclarationByItemName.ContainsKey(sku)) // awful!
            { return _autocompleteData.CustomsDeclarationByItemName[sku]; }
            return string.Empty;
        }

        private string GetShippingType(string shipCountry, string sku)
        {
            string defaultShippingName = DefaultShippingNames.First();
            if (!_euContries.Contains(shipCountry))
            {
                return defaultShippingName;
            }

            if (!_autocompleteData.ShippingNameByItemName.ContainsKey(sku))
            {
                return defaultShippingName;
            }

            return _autocompleteData.ShippingNameByItemName[sku];
        }

        public void ProcessInvoices(string fileName)
        {
            var invoice = _invoicesXmlManager.PrepareInvoice(_convertedInvoices);

            FixItems(InvoiceItemsAll);
            if (_convertedInvoices == null || !_convertedInvoices.Any())
            {
                //TODO MessageBox.Show("Zadne faktury nebyly konvertovany!");
                return;
            }

            _invoicesXmlManager.SerializeXmlInvoice(fileName, invoice);
            UpdateExistingInvoiceNumber(invoice);

            _autocompleteDataLoader.SaveSettings(_autocompleteData);
        }

        private string GetItemCodeBySku(string sku)
        {
            string defaultCode = "----";
            if (!_autocompleteData.PohodaProdCodeByAmazonProdCode.ContainsKey(sku)) return defaultCode;

            return _autocompleteData.PohodaProdCodeByAmazonProdCode[sku];
        }

        private string GetSavedCenterBySku(string sku)
        {
            string defaultCentreIds = "----";
            if (!_autocompleteData.ProdWarehouseSectionByAmazonProdCode.ContainsKey(sku)) return defaultCentreIds;

            return _autocompleteData.ProdWarehouseSectionByAmazonProdCode[sku];
        }

        private void UpdateExistingInvoiceNumber(InvoiceXML.dataPack invoice)
        {
            ExistingInvoiceNumber += (uint)(invoice.dataPackItem).Count();
        }

        private static DateTime CalculateTaxDate(DateTime conversionDate, string classification)
        {
            if (classification == "UVzboží")
                return conversionDate.AddDays(5.0);
            return conversionDate;
        }

        private static DateTime CalculateDueDate(DateTime purchaseDate)
        {
            return purchaseDate.AddDays(3.0);
        }

        private string SetClassification(string shipCountry)
        {
            return (_euContries).Contains(shipCountry) ? "UDA5" : "UVzboží"; // UDA5 pouze CZ, UVzboží - zustane, UDzasEU - bude zbyle staty Europe
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

        private static string FormatNameOfItem(string itemName)
        {
            return itemName; // FIXME 
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
