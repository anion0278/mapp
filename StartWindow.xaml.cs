using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using Martin_App;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using Newtonsoft.Json;
using Formatting = System.Xml.Formatting;
using Path = System.IO.Path;

namespace Martin_app
{
    public partial class StartWindow : Window
    {
        private static readonly int MaxItemNameLength = 85;
        private uint _existingInvoceNumber = Settings.Default.ExistingInvoceNumber;
        private decimal _dphValue = Settings.Default.DPH;
        private readonly int MaxAddressNameLength = 60;
        private readonly int MaxClientNameLength = 30;
        private IEnumerable<InvoiceXML.dataPackDataPackItem> _convertedInvoices;
        private Dictionary<string, decimal> Rates;

        private Encoding XmlEncoding { get; } = CodePagesEncodingProvider.Instance.GetEncoding("windows-1250");

        public ObservableCollection<InvoiceXML.dataPackDataPackItem> InvoicesTable { get; set; } = new ObservableCollection<InvoiceXML.dataPackDataPackItem>();

        public ObservableCollection<InvoiceItemWithDetailes> InvoiceItemsAll { get; set; } = new ObservableCollection<InvoiceItemWithDetailes>();

        public Dictionary<string, string> ProductNumberByName { get; set; }
        private string ProductNumberByNameJson = "ProductNumberByName.json";

        public Dictionary<string, string> ProductCodeByAmazonName { get; set; }
        private string ProductCodeByAmazonNameJson = "ProductCodeByAmazonName.json";

        private uint ExistingInvoceNumber
        {
            get
            {
                return _existingInvoceNumber;
            }
            set
            {
                if ((int)value == (int)_existingInvoceNumber) return;

                _existingInvoceNumber = value;
                Settings.Default.ExistingInvoceNumber = _existingInvoceNumber;
                Settings.Default.Save();
                ExistingInvoiceNum.Text = ExistingInvoceNumber.ToString();
            }
        }

        private decimal DPH
        {
            get
            {
                return _dphValue;
            }
            set
            {
                if (value == _dphValue)
                    return;
                _dphValue = value;
                Settings.Default.DPH = _dphValue;
                Settings.Default.Save();
            }
        }

        public StartWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Initialize();
        }

        private void ConvertAmazonToPohoda()
        {
            var fromAmazonReports = GetParametersFromAmazonReport();
            if (fromAmazonReports == null) return;

            var source = new List<InvoiceXML.dataPackDataPackItem>();
            foreach (var report in fromAmazonReports)
            {
                var singleAmazonInvoice = FillDataPackItem(report, source.Count + 1);
                var existingDataPack = source.FirstOrDefault((di => di.invoice.invoiceHeader.symPar == singleAmazonInvoice.invoice.invoiceHeader.symPar));
                if (existingDataPack != null) AddItemsToExistingDataPack(existingDataPack, singleAmazonInvoice);
                else source.Add(singleAmazonInvoice);
            }

            var invoiceInvoiceItems = source.SelectMany((di => di.invoice.invoiceDetail));
            InvoiceItemsAll.Clear();
            foreach (var invoiceInvoiceItem in invoiceInvoiceItems)
            {
                var item = invoiceInvoiceItem;
                InvoiceItemsAll.Add(new InvoiceItemWithDetailes(item, source.Single((di => (di.invoice.invoiceDetail).Contains<InvoiceXML.invoiceInvoiceItem>(item))).invoice.invoiceHeader));
            }

            InvoicesTable.Clear();
            foreach (var packDataPackItem in source) InvoicesTable.Add(packDataPackItem);
            _convertedInvoices = source;
        }

        private void AddItemsToExistingDataPack(
          InvoiceXML.dataPackDataPackItem existingDataPack,
          InvoiceXML.dataPackDataPackItem dataPackDataPackItem)
        {
            var list = (existingDataPack.invoice.invoiceDetail).ToList();
            var invoiceInvoiceItem = (dataPackDataPackItem.invoice.invoiceDetail).First();
            list.Insert(list.Count - 1, invoiceInvoiceItem);
            var shippingItem1 = GetShippingItem(list);
            var shippingItem2 = GetShippingItem(dataPackDataPackItem.invoice.invoiceDetail);
            shippingItem1.foreignCurrency.unitPrice += shippingItem2.foreignCurrency.unitPrice;
            shippingItem1.foreignCurrency.price += shippingItem2.foreignCurrency.price;
            shippingItem1.foreignCurrency.priceSum += shippingItem2.foreignCurrency.priceSum;
            shippingItem1.foreignCurrency.priceVAT += shippingItem2.foreignCurrency.priceVAT;
            shippingItem1.homeCurrency.unitPrice += shippingItem2.homeCurrency.unitPrice;
            shippingItem1.homeCurrency.price += shippingItem2.homeCurrency.price;
            shippingItem1.homeCurrency.priceSum += shippingItem2.homeCurrency.priceSum;
            shippingItem1.homeCurrency.priceVAT += shippingItem2.homeCurrency.priceVAT;
            existingDataPack.invoice.invoiceDetail = list.ToArray();
        }

        private static InvoiceXML.invoiceInvoiceItem GetShippingItem(
          IEnumerable<InvoiceXML.invoiceInvoiceItem> existingItems)
        {
            return existingItems.Single((i =>
            {
                string text = i.text;
                if (text == null) return false;

                return text.ToLower().Contains("shipping");
            }));
        }

        private void Initialize()
        {
            ExistingInvoiceNum.Text = Settings.Default.ExistingInvoceNumber.ToString();
            DPHValue.Text = Settings.Default.DPH.ToString();
            Rates = GetActualCurrencyRates();

            string json = File.ReadAllText(ProductCodeByAmazonNameJson);
            ProductCodeByAmazonName = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            json = File.ReadAllText(ProductNumberByNameJson);
            ProductNumberByName = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        private Dictionary<string, decimal> GetActualCurrencyRates()
        {
            string downloadString;
            using (WebClient webClient = new WebClient() { Encoding = XmlEncoding })
            {
                string str = DateTime.Today.AddDays(-1.0).ToString("dd.MM.yyyy");
                downloadString = webClient.DownloadString("http://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt?date=" + str);
            }
            if (string.IsNullOrEmpty(downloadString))
                throw new ArgumentNullException("Could not get Currency Rate!");

            var strArrayList = new List<string[]>();
            using (TextFieldParser textFieldParser = new TextFieldParser(new StringReader(downloadString)))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters("|");
                while (!textFieldParser.EndOfData)
                {
                    string[] strArray = textFieldParser.ReadFields();
                    strArrayList.Add(strArray);
                }
            }
            Dictionary<string, decimal> dictionary = new Dictionary<string, decimal>();
            for (int index = 2; index < strArrayList.Count; ++index)
            {
                decimal num = decimal.Parse(ToInvariantFormat(strArrayList[index][4])) / decimal.Parse(ToInvariantFormat(strArrayList[index][2]));
                dictionary.Add(strArrayList[index][3], num);
            }
            return dictionary;
        }

        private static string ToInvariantFormat(string input)
        {
            return input.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
        }


        private InvoiceXML.dataPackDataPackItem FillDataPackItem(Dictionary<string, string> valuesFromAmazon, int index)
        {
            string invoiceType = "issuedInvoice";
            string accountingIds = "3Fv";

            string shipping = "Shipping";
            string headerSymPar = valuesFromAmazon["order-id"];
            uint invoiceNumber = (uint)(ExistingInvoceNumber + index);
            var packDataPackItem = PrepareDatapackItem();
            string userId = "Usr01 (" + index.ToString().PadLeft(3, '0') + ")";
            decimal itemPrice = decimal.Parse(valuesFromAmazon["item-price"]);
            decimal shippingPrice = decimal.Parse(valuesFromAmazon["shipping-price"]);
            decimal promotionDiscount = decimal.Parse(valuesFromAmazon["item-promotion-discount"]);
            decimal shipPromotionDiscount = decimal.Parse(valuesFromAmazon["ship-promotion-discount"]);
            string currency = CurrencyConverter.Convert(valuesFromAmazon["currency"]);
            decimal currencyRate = GetCurrencyRate(currency);
            decimal priceSum = itemPrice + shippingPrice - promotionDiscount - shipPromotionDiscount;
            decimal currencyPriceHighSum = priceSum * currencyRate;
            string salesChannel = valuesFromAmazon["sales-channel"];
            string clientName = FormatClientName(valuesFromAmazon["recipient-name"], headerSymPar);
            string city = FormatCity(valuesFromAmazon["ship-city"], valuesFromAmazon["ship-state"], valuesFromAmazon["ship-postal-code"], headerSymPar);
            string fullAddress = FormatFullAddress(valuesFromAmazon["ship-address-1"], valuesFromAmazon["ship-address-2"], valuesFromAmazon["ship-address-3"], headerSymPar);
            string phoneNumber = FormatPhoneNumber(valuesFromAmazon["ship-phone-number"], valuesFromAmazon["buyer-phone-number"], headerSymPar);
            string shipCountry = valuesFromAmazon["ship-country"];
            string classification = SetClassification(shipCountry);

            string productName = FormatNameOfItem(valuesFromAmazon["product-name"]);
            string stockItemIds = GetItemCodeByName(productName);

            //FormatDate(valuesFromAmazon["purchase-date"]);
            DateTime today = DateTime.Today;
            DateTime taxDate = CalculateTaxDate(today, classification);
            DateTime dueDate = CalculateDueDate(today);
            packDataPackItem.id = userId;
            packDataPackItem.invoice.invoiceHeader.invoiceType = invoiceType;
            packDataPackItem.invoice.invoiceHeader.number.numberRequested = invoiceNumber;
            packDataPackItem.invoice.invoiceHeader.symVar = invoiceNumber;
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
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.city = city;
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.street = fullAddress;
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.country.ids = shipCountry;
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.phone = phoneNumber;
            packDataPackItem.invoice.invoiceHeader.paymentType.ids = salesChannel;
            packDataPackItem.invoice.invoiceHeader.centre.ids = GetSavedCenter(productName);
            packDataPackItem.invoice.invoiceHeader.liquidation.amountHome = priceSum * currencyRate;
            packDataPackItem.invoice.invoiceHeader.liquidation.amountForeign = priceSum;
            var invoiceInvoiceItemList = new List<InvoiceXML.invoiceInvoiceItem>();
            var invoiceInvoiceItem1 = new InvoiceXML.invoiceInvoiceItem();
            decimal num8 = decimal.Parse(valuesFromAmazon["quantity-purchased"]);
            invoiceInvoiceItem1.quantity = num8;
            invoiceInvoiceItem1.payVAT = true;
            invoiceInvoiceItem1.discountPercentage = decimal.Zero;
            invoiceInvoiceItem1.foreignCurrency = new InvoiceXML.invoiceInvoiceItemForeignCurrency();
            invoiceInvoiceItem1.homeCurrency = new InvoiceXML.invoiceInvoiceItemHomeCurrency();
            invoiceInvoiceItem1.stockItem = new InvoiceXML.invoiceInvoiceItemStockItem();
            invoiceInvoiceItem1.stockItem.stockItem = new InvoiceXML.stockItem();
            invoiceInvoiceItem1.stockItem.store = new InvoiceXML.store();
            if (classification == "UVzboží")
            {
                invoiceInvoiceItem1.rateVAT = "none";
                invoiceInvoiceItem1.foreignCurrency.unitPrice = itemPrice / num8;
                invoiceInvoiceItem1.foreignCurrency.price = invoiceInvoiceItem1.foreignCurrency.unitPrice * num8;
                invoiceInvoiceItem1.foreignCurrency.priceSum = invoiceInvoiceItem1.foreignCurrency.price;
                invoiceInvoiceItem1.foreignCurrency.priceVAT = decimal.Zero;
                invoiceInvoiceItem1.homeCurrency.unitPrice = invoiceInvoiceItem1.foreignCurrency.unitPrice * currencyRate;
                invoiceInvoiceItem1.homeCurrency.price = invoiceInvoiceItem1.foreignCurrency.price * currencyRate;
                invoiceInvoiceItem1.homeCurrency.priceSum *= currencyRate;
                invoiceInvoiceItem1.homeCurrency.priceVAT = decimal.Zero;
            }
            else
            {
                invoiceInvoiceItem1.rateVAT = "high";
                invoiceInvoiceItem1.foreignCurrency.unitPrice = itemPrice / num8;
                invoiceInvoiceItem1.foreignCurrency.price = invoiceInvoiceItem1.foreignCurrency.unitPrice * num8 * (decimal.One - DPH);
                invoiceInvoiceItem1.foreignCurrency.priceVAT = invoiceInvoiceItem1.foreignCurrency.unitPrice * num8 * DPH;
                invoiceInvoiceItem1.foreignCurrency.priceSum = invoiceInvoiceItem1.foreignCurrency.unitPrice * num8;
                invoiceInvoiceItem1.homeCurrency.unitPrice = invoiceInvoiceItem1.foreignCurrency.unitPrice * currencyRate;
                invoiceInvoiceItem1.homeCurrency.price = invoiceInvoiceItem1.foreignCurrency.price * currencyRate;
                invoiceInvoiceItem1.homeCurrency.priceVAT = invoiceInvoiceItem1.foreignCurrency.priceVAT * currencyRate;
                invoiceInvoiceItem1.homeCurrency.priceSum = invoiceInvoiceItem1.foreignCurrency.priceSum * currencyRate;
            }
            invoiceInvoiceItem1.code = stockItemIds;
            invoiceInvoiceItem1.text = productName;
            invoiceInvoiceItem1.stockItem.store.ids = "Zboží";
            invoiceInvoiceItem1.stockItem.stockItem.ids = stockItemIds;
            invoiceInvoiceItem1.PDP = false;
            var invoiceInvoiceItem2 = new InvoiceXML.invoiceInvoiceItem();
            invoiceInvoiceItem2.text = shipping;
            invoiceInvoiceItem2.quantity = decimal.One;
            invoiceInvoiceItem2.payVAT = true;
            invoiceInvoiceItem2.discountPercentage = decimal.Zero;
            invoiceInvoiceItem2.foreignCurrency = new InvoiceXML.invoiceInvoiceItemForeignCurrency();
            invoiceInvoiceItem2.homeCurrency = new InvoiceXML.invoiceInvoiceItemHomeCurrency();
            if (classification == "UVzboží")
            {
                invoiceInvoiceItem2.rateVAT = "none";
                invoiceInvoiceItem2.foreignCurrency.unitPrice = shippingPrice;
                invoiceInvoiceItem2.foreignCurrency.price = invoiceInvoiceItem2.foreignCurrency.unitPrice;
                invoiceInvoiceItem2.foreignCurrency.priceSum = invoiceInvoiceItem2.foreignCurrency.price;
                invoiceInvoiceItem2.foreignCurrency.priceVAT = decimal.Zero;
                invoiceInvoiceItem2.homeCurrency.unitPrice = invoiceInvoiceItem2.foreignCurrency.unitPrice * currencyRate;
                invoiceInvoiceItem2.homeCurrency.price = invoiceInvoiceItem2.foreignCurrency.price * currencyRate;
                invoiceInvoiceItem2.homeCurrency.priceSum *= currencyRate;
                invoiceInvoiceItem2.homeCurrency.priceVAT = decimal.Zero;
            }
            else
            {
                invoiceInvoiceItem2.rateVAT = "high";
                invoiceInvoiceItem2.foreignCurrency.unitPrice = shippingPrice;
                invoiceInvoiceItem2.foreignCurrency.price = invoiceInvoiceItem2.foreignCurrency.unitPrice * num8 * (decimal.One - DPH);
                invoiceInvoiceItem2.foreignCurrency.priceVAT = invoiceInvoiceItem2.foreignCurrency.unitPrice * num8 * DPH;
                invoiceInvoiceItem2.foreignCurrency.priceSum = invoiceInvoiceItem2.foreignCurrency.unitPrice * num8;
                invoiceInvoiceItem2.homeCurrency.unitPrice = invoiceInvoiceItem2.foreignCurrency.unitPrice * currencyRate;
                invoiceInvoiceItem2.homeCurrency.price = invoiceInvoiceItem2.foreignCurrency.price * currencyRate;
                invoiceInvoiceItem2.homeCurrency.priceVAT = invoiceInvoiceItem2.foreignCurrency.priceVAT * currencyRate;
                invoiceInvoiceItem2.homeCurrency.priceSum = invoiceInvoiceItem2.foreignCurrency.priceSum * currencyRate;
            }
            invoiceInvoiceItem2.PDP = false;
            invoiceInvoiceItemList.Add(invoiceInvoiceItem1);
            invoiceInvoiceItemList.Add(invoiceInvoiceItem2);
            if (promotionDiscount != decimal.Zero)
            {
                var invoiceInvoiceItem3 = new InvoiceXML.invoiceInvoiceItem();
                invoiceInvoiceItem3.text = "Discount";
                invoiceInvoiceItem3.quantity = decimal.One;
                invoiceInvoiceItem3.payVAT = true;
                invoiceInvoiceItem3.discountPercentage = decimal.Zero;
                invoiceInvoiceItem3.foreignCurrency = new InvoiceXML.invoiceInvoiceItemForeignCurrency();
                invoiceInvoiceItem3.homeCurrency = new InvoiceXML.invoiceInvoiceItemHomeCurrency();
                if (classification == "UVzboží")
                {
                    invoiceInvoiceItem3.rateVAT = "none";
                    invoiceInvoiceItem3.foreignCurrency.unitPrice = promotionDiscount;
                    invoiceInvoiceItem3.foreignCurrency.price = invoiceInvoiceItem3.foreignCurrency.unitPrice;
                    invoiceInvoiceItem3.foreignCurrency.priceSum = invoiceInvoiceItem3.foreignCurrency.price;
                    invoiceInvoiceItem3.foreignCurrency.priceVAT = decimal.Zero;
                    invoiceInvoiceItem3.homeCurrency.unitPrice = invoiceInvoiceItem3.foreignCurrency.unitPrice * currencyRate;
                    invoiceInvoiceItem3.homeCurrency.price = invoiceInvoiceItem3.foreignCurrency.price * currencyRate;
                    invoiceInvoiceItem3.homeCurrency.priceSum *= currencyRate;
                    invoiceInvoiceItem3.homeCurrency.priceVAT = decimal.Zero;
                }
                else
                {
                    invoiceInvoiceItem3.rateVAT = "high";
                    invoiceInvoiceItem3.foreignCurrency.unitPrice = promotionDiscount;
                    invoiceInvoiceItem3.foreignCurrency.price = invoiceInvoiceItem3.foreignCurrency.unitPrice * num8 * (decimal.One - DPH);
                    invoiceInvoiceItem3.foreignCurrency.priceVAT = invoiceInvoiceItem3.foreignCurrency.unitPrice * num8 * DPH;
                    invoiceInvoiceItem3.foreignCurrency.priceSum = invoiceInvoiceItem3.foreignCurrency.unitPrice * num8;
                    invoiceInvoiceItem3.homeCurrency.unitPrice = invoiceInvoiceItem3.foreignCurrency.unitPrice * currencyRate;
                    invoiceInvoiceItem3.homeCurrency.price = invoiceInvoiceItem3.foreignCurrency.price * currencyRate;
                    invoiceInvoiceItem3.homeCurrency.priceVAT = invoiceInvoiceItem3.foreignCurrency.priceVAT * currencyRate;
                    invoiceInvoiceItem3.homeCurrency.priceSum = invoiceInvoiceItem3.foreignCurrency.priceSum * currencyRate;
                }
                invoiceInvoiceItem3.PDP = false;
                invoiceInvoiceItemList.Add(invoiceInvoiceItem3);
            }
            packDataPackItem.invoice.invoiceDetail = invoiceInvoiceItemList.ToArray();
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceNone = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceLow = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceLowVAT = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceLowSum = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHigh = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighVAT = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighSum = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.price3 = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.price3VAT = decimal.Zero;
            packDataPackItem.invoice.invoiceSummary.homeCurrency.price3Sum = decimal.Zero;
            if (classification == "UVzboží")
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

        private string GetItemCodeByName(string productName)
        {
            string defaultCode = "1108A";
            if (!ProductCodeByAmazonName.ContainsKey(productName)) return defaultCode;

            return ProductCodeByAmazonName[productName];
        }

        private string GetSavedCenter(string productName)
        {
            string defaultCentreIds = "DER";
            if (!ProductNumberByName.ContainsKey(productName)) return defaultCentreIds;

            return ProductNumberByName[productName];
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

            str = AskToChangeLongStringIfNeeded(message, str, MaxAddressNameLength);

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
            string message = "Jmeno zakazniku je prilis dlouhe v objednavce C.: "
                             + amazonOrderNumber;

            name = AskToChangeLongStringIfNeeded(message, name, MaxClientNameLength);

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
            str = AskToChangeLongStringIfNeeded(message, str, MaxAddressNameLength);
            return str;
        }

        private string AskToChangeLongStringIfNeeded(string message, string str, int maxLength)
        {
            message += $". Upravit manualne (Yes), nebo orezat dle maximalni delky {maxLength} (No)";
            while (str.Length > maxLength)
            {
                var result = MessageBox.Show(message, "Upozorneni", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var window = new ManualChange(maxLength, str);
                    window.ShowDialog();
                    str = window.CorrectedText;
                }
                else
                {
                    str = str.Substring(0, maxLength);
                }
            }

            return str;
        }

        private static string FormatNameOfItem(string itemName)
        {
            return itemName;
        }

        private decimal GetCurrencyRate(string currency)
        {
            return Rates.Single((r => r.Key.Equals(currency, StringComparison.InvariantCultureIgnoreCase))).Value;
        }

        private InvoiceXML.dataPackDataPackItem PrepareDatapackItem()
        {
            InvoiceXML.dataPack dataPack;
            using (StreamReader streamReader = new StreamReader("InvoiceBasic", XmlEncoding))
                dataPack = (InvoiceXML.dataPack)new XmlSerializer(typeof(InvoiceXML.dataPack)).Deserialize((TextReader)streamReader);
            return dataPack.dataPackItem[0];
        }

        private void ExportToXML(IEnumerable<InvoiceXML.dataPackDataPackItem> dataItems)
        {
            var invoice = PrepareInvoice(dataItems);
            var settings = new XmlWriterSettings();
            settings.Encoding = XmlEncoding;
            settings.NamespaceHandling = NamespaceHandling.Default;
            settings.Indent = true;
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Zvol vystupni slozku",
                FileName = "PohodaInvoices_" + DateAndTime.Today.ToString("dd-MM-yyyy") + ".xml"
            };
            bool? dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult != true) return;

            var xmlSerializer = new XmlSerializer(invoice.GetType());
            using (var xmlWriter = XmlWriter.Create(saveFileDialog.FileName, settings))
            {
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("dat", "http://www.stormware.cz/schema/version_2/data.xsd");
                xmlSerializer.Serialize(xmlWriter, invoice, namespaces);
            }
            FixNamespaces(saveFileDialog.FileName);
            UpdateExistingInvoiceNumber(invoice);
        }

        private void UpdateExistingInvoiceNumber(InvoiceXML.dataPack invoice)
        {
            ExistingInvoceNumber += (uint)(invoice.dataPackItem).Count();
        }

        private void FixNamespaces(string resultFilePath)
        {
            File.WriteAllText(resultFilePath,
                File.ReadAllText(resultFilePath, XmlEncoding)
                .Replace("5448034", "05448034")
                .Replace("<inv:invoiceItem xmlns:typ=\"http://www.stormware.cz/schema/version_2/type.xsd\">", "<inv:invoiceItem>")
                .Replace("<inv:invoiceDetail>", "<inv:invoiceDetail xmlns:rsp=\"http://www.stormware.cz/schema/version_2/response.xsd\" xmlns:rdc=\"http://www.stormware.cz/schema/version_2/documentresponse.xsd\" xmlns:typ=\"http://www.stormware.cz/schema/version_2/type.xsd\" xmlns:lst=\"http://www.stormware.cz/schema/version_2/list.xsd\" xmlns:lStk=\"http://www.stormware.cz/schema/version_2/list_stock.xsd\" xmlns:lAdb=\"http://www.stormware.cz/schema/version_2/list_addBook.xsd\" xmlns:lCen=\"http://www.stormware.cz/schema/version_2/list_centre.xsd\" xmlns:lAcv=\"http://www.stormware.cz/schema/version_2/list_activity.xsd\" xmlns:acu=\"http://www.stormware.cz/schema/version_2/accountingunit.xsd\" xmlns:vch=\"http://www.stormware.cz/schema/version_2/voucher.xsd\" xmlns:int=\"http://www.stormware.cz/schema/version_2/intDoc.xsd\" xmlns:stk=\"http://www.stormware.cz/schema/version_2/stock.xsd\" xmlns:ord=\"http://www.stormware.cz/schema/version_2/order.xsd\" xmlns:ofr=\"http://www.stormware.cz/schema/version_2/offer.xsd\" xmlns:enq=\"http://www.stormware.cz/schema/version_2/enquiry.xsd\" xmlns:vyd=\"http://www.stormware.cz/schema/version_2/vydejka.xsd\" xmlns:pri=\"http://www.stormware.cz/schema/version_2/prijemka.xsd\" xmlns:bal=\"http://www.stormware.cz/schema/version_2/balance.xsd\" xmlns:pre=\"http://www.stormware.cz/schema/version_2/prevodka.xsd\" xmlns:vyr=\"http://www.stormware.cz/schema/version_2/vyroba.xsd\" xmlns:pro=\"http://www.stormware.cz/schema/version_2/prodejka.xsd\" xmlns:con=\"http://www.stormware.cz/schema/version_2/contract.xsd\" xmlns:adb=\"http://www.stormware.cz/schema/version_2/addressbook.xsd\" xmlns:prm=\"http://www.stormware.cz/schema/version_2/parameter.xsd\" xmlns:lCon=\"http://www.stormware.cz/schema/version_2/list_contract.xsd\" xmlns:ctg=\"http://www.stormware.cz/schema/version_2/category.xsd\" xmlns:ipm=\"http://www.stormware.cz/schema/version_2/intParam.xsd\" xmlns:str=\"http://www.stormware.cz/schema/version_2/storage.xsd\" xmlns:idp=\"http://www.stormware.cz/schema/version_2/individualPrice.xsd\" xmlns:sup=\"http://www.stormware.cz/schema/version_2/supplier.xsd\" xmlns:prn=\"http://www.stormware.cz/schema/version_2/print.xsd\" xmlns:sEET=\"http://www.stormware.cz/schema/version_2/sendEET.xsd\" xmlns:act=\"http://www.stormware.cz/schema/version_2/accountancy.xsd\" xmlns:bnk=\"http://www.stormware.cz/schema/version_2/bank.xsd\" xmlns:sto=\"http://www.stormware.cz/schema/version_2/store.xsd\" xmlns:grs=\"http://www.stormware.cz/schema/version_2/groupStocks.xsd\" xmlns:acp=\"http://www.stormware.cz/schema/version_2/actionPrice.xsd\" xmlns:csh=\"http://www.stormware.cz/schema/version_2/cashRegister.xsd\" xmlns:bka=\"http://www.stormware.cz/schema/version_2/bankAccount.xsd\" xmlns:ilt=\"http://www.stormware.cz/schema/version_2/inventoryLists.xsd\" xmlns:nms=\"http://www.stormware.cz/schema/version_2/numericalSeries.xsd\" xmlns:pay=\"http://www.stormware.cz/schema/version_2/payment.xsd\" xmlns:mKasa=\"http://www.stormware.cz/schema/version_2/mKasa.xsd\" xmlns:gdp=\"http://www.stormware.cz/schema/version_2/GDPR.xsd\" xmlns:est=\"http://www.stormware.cz/schema/version_2/establishment.xsd\" xmlns:cen=\"http://www.stormware.cz/schema/version_2/centre.xsd\" xmlns:acv=\"http://www.stormware.cz/schema/version_2/activity.xsd\" xmlns:ftr=\"http://www.stormware.cz/schema/version_2/filter.xsd\">"), XmlEncoding);
        }

        private static InvoiceXML.dataPack PrepareInvoice(IEnumerable<InvoiceXML.dataPackDataPackItem> dataItems)
        {
            return new InvoiceXML.dataPack()
            {
                ico = 5448034,
                id = "Usr01",
                key = "efd0db6a-9c08-4bb5-befb-36809eadcba1",
                programVersion = "12101.4 (7.1.2019)",
                application = "Transformace",
                note = "Uživatelský export",
                dataPackItem = dataItems.ToArray(),
                version = new decimal(20, 0, 0, false, (byte)1)
            };
        }

        private static DateTime FormatDate(string dataString)
        {
            return DateTime.Parse(dataString.Remove(10, dataString.Length - 10));
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

        private static string SetClassification(string shipCountry)
        {
            var euContries = new[]
            {
                "BE", "BG", "CZ", "DK", "EE", "FI", "FR", "IE", "IT", "CY", "LT", "LV", "LU", "HU", "HR", "MT", "DE",
                "NL", "PL", "PT", "AT", "RO", "GR", "SK", "SI", "ES", "SE", "GB", "EU"
            };
            return (euContries).Contains(shipCountry) ? "UDA5" : "UVzboží";
        }


        private static List<Dictionary<string, string>> GetParametersFromAmazonReport()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Zvol Amazon report",
                Filter = "Amazon Report|*.txt"
            };
            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == false) return null;

            //string fileName = openFileDialog.FileName;

            var dictList = new List<Dictionary<string, string>>();
            foreach (var fileName in openFileDialog.FileNames)
            {
                var validLines = GetOrderDataLinesFromSingleFile(fileName);

                for (int lineIndex = 1; lineIndex < validLines.Count; lineIndex++)
                {
                    var invoiceDict = new Dictionary<string, string>();
                    for (int columnIndex = 0; columnIndex < validLines[0].Length; ++columnIndex)
                    {
                        string columnNameKey = validLines[0][columnIndex];
                        invoiceDict.Add(columnNameKey, validLines[lineIndex][columnIndex]);
                    }
                    dictList.Add(invoiceDict);
                }
            }

            return dictList;
        }

        private static List<string[]> GetOrderDataLinesFromSingleFile(string fileName)
        {
            var lineItems = new List<string[]>();
            using (var textFieldParser = new TextFieldParser(fileName))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters("\t");
                while (!textFieldParser.EndOfData)
                {
                    string[] orderLine = textFieldParser.ReadFields();
                    lineItems.Add(orderLine);
                }
            }

            var validLines = new List<string[]>();
            // remove empty lines
            for (var orderLineIndex = 0; orderLineIndex < lineItems.Count; orderLineIndex++)
            {
                var orderLine = lineItems[orderLineIndex];
                if (orderLine.Count(string.IsNullOrEmpty) > orderLine.Length / 2)
                {
                    MessageBox.Show(
                        $"Objednavka {orderLine[0]} na radku {orderLineIndex} " +
                        $"ze souboru \'{Path.GetFileName(fileName)}\' obsahuje neplatny zaznam (prazdny). " +
                        "Zaznam bude odstranen.");
                    continue;
                }

                validLines.Add(orderLine);
            }

            return validLines;
        }

        private void DPHValue_LostFocus(object sender, RoutedEventArgs e)
        {
            decimal dphValue;
            try
            {
                dphValue = decimal.Parse(ToInvariantFormat(DPHValue.Text));
            }
            catch (Exception ex)
            {
                ButtonConvert.IsEnabled = false;
                MessageBox.Show("DPH je zadano spatne!");
                return;
            }
            DPH = dphValue;
        }

        private void ExistingInvoiceNum_LostFocus(object sender, RoutedEventArgs e)
        {
            uint existingInvoceNumber;
            try
            {
                existingInvoceNumber = uint.Parse(ToInvariantFormat(ExistingInvoiceNum.Text));
            }
            catch (Exception ex)
            {
                ButtonConvert.IsEnabled = false;
                MessageBox.Show("Invoice number je zadan spatne!");
                return;
            }
            ExistingInvoceNumber = existingInvoceNumber;
        }

        public void ButtonConvert_Click(object sender, RoutedEventArgs e)
        {
            ConvertAmazonToPohoda();
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            FixItems(InvoiceItemsAll);
            var convertedInvoices = _convertedInvoices;
            if (convertedInvoices != null && convertedInvoices.Any())
            {
                ExportToXML(_convertedInvoices);
            }
            else
            {
                MessageBox.Show("Zadne faktury nebyly konvertovany!");
            }
        }

        private void FixItems(IEnumerable<InvoiceItemWithDetailes> invoiceItemsAll)
        {
            foreach (var itemWithDetailes in invoiceItemsAll)
            {
                if (itemWithDetailes.Item.stockItem != null)
                    itemWithDetailes.Item.stockItem.stockItem.ids = itemWithDetailes.Item.code;
                if (itemWithDetailes.Item.text.Length > MaxItemNameLength)
                    itemWithDetailes.Item.text = itemWithDetailes.Item.text.Substring(0, MaxItemNameLength);
            }
        }

        private void TopDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (!(e.Column is DataGridBoundColumn)) return;

                if (e.Column.DisplayIndex == 3)
                {
                    string productCode = (e.EditingElement as TextBox).Text;
                    var dataContextItem = (InvoiceItemWithDetailes) e.Row.DataContext;
                    string productName = dataContextItem.Item.text;
                    if (ProductCodeByAmazonName.ContainsKey(productName))
                    {
                        if (!ProductCodeByAmazonName[productName].Equals(productCode)) ProductCodeByAmazonName[productName] = productCode;
                    }
                    else
                    {
                        ProductCodeByAmazonName.Add(productName, productCode);
                    }
                }
            }
        }

        private void BottomDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (!(e.Column is DataGridBoundColumn)) return;

                if (e.Column.DisplayIndex == 3)
                {
                    string productNumber = (e.EditingElement as TextBox).Text;
                    var dataContextItem = (InvoiceXML.dataPackDataPackItem)e.Row.DataContext;
                    string productName = dataContextItem.invoice.invoiceDetail.FirstOrDefault()?.text ?? string.Empty;
                    if (ProductNumberByName.ContainsKey(productName))
                    {
                        if (!ProductNumberByName[productName].Equals(productNumber)) ProductNumberByName[productName] = productNumber;
                    }
                    else
                    {
                        ProductNumberByName.Add(productName, productNumber);
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string json = JsonConvert.SerializeObject(ProductCodeByAmazonName);
            File.WriteAllText(ProductCodeByAmazonNameJson, json);

            json = JsonConvert.SerializeObject(ProductNumberByName);
            File.WriteAllText(ProductNumberByNameJson, json);
        }
    }
}
