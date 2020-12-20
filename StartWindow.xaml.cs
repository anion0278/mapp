using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using WindowsInput;
using WindowsInput.Native;
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
        private uint _existingInvoceNumber;
        private decimal _dphValue;
        private readonly int MaxAddressNameLength = 60;
        private readonly int MaxCityLength = 45;
        private readonly int MaxClientNameLength = 30;
        private IEnumerable<InvoiceXML.dataPackDataPackItem> _convertedInvoices;
        private Dictionary<string, decimal> Rates;

        private Encoding XmlEncoding { get; } = CodePagesEncodingProvider.Instance.GetEncoding("windows-1250");

        public ObservableCollection<InvoiceXML.dataPackDataPackItem> InvoicesTable { get; set; } = new ObservableCollection<InvoiceXML.dataPackDataPackItem>();

        public ObservableCollection<InvoiceItemWithDetails> InvoiceItemsAll { get; set; } = new ObservableCollection<InvoiceItemWithDetails>();

        public string[] _euContries =
        {
            "BE", "BG", "CZ", "DK", "EE", "FI", "FR", "IE", "IT", "CY", "LT", "LV", "LU", "HU", "HR", "MT", "DE",
            "NL", "PL", "PT", "AT", "RO", "GR", "SK", "SI", "ES", "SE", "GB", "EU"
        };

        private string[] DefaultShippingNames = { "Shipping", "Registered shipping" };

        public Dictionary<string, string> ProductNumberByItemName { get; set; }
        private string ProductNumberByItemNameJson = "ProductNumberByName.json";

        public Dictionary<string, string> ProductCodeByItemName { get; set; }
        private string ProductCodeByItemNameJson = "ProductCodeByAmazonName.json";

        public Dictionary<string, string> ShippingNameByItemName { get; set; }
        private string ShippingNameByItemNameJson = "ShippingNameByItemName.json";

        public Dictionary<string, string> PackQuantityByItemName { get; set; }
        private string ProductQuantityByItemNameJson = "ProductQuantityByAmazonName.json";

        public Dictionary<string, string> CustomsDeclarationByItemName { get; set; }
        private string CustomsDeclarationByItemNameJson = "CustomsDeclarationByAmazonName.json";

        KeyboardHook _keyboardHook;

        private bool _isCommandPressed = false;

        private InputSimulator _keyboardSim = new InputSimulator();

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

        private string _defaultEmail;
        private string DefaultEmail
        {
            get
            {
                return _defaultEmail;
            }
            set
            {
                if (value == _defaultEmail) return;

                _defaultEmail = value;
                Settings.Default.DefaultEmail = _defaultEmail;
                Settings.Default.Save();
                DefaultEmailBox.Text = DefaultEmail;
            }
        }

        private string _latestTrackingCode;
        private string LatestTrackingCode
        {
            get
            {
                return _latestTrackingCode;
            }
            set
            {
                if (value == _latestTrackingCode) return;

                _latestTrackingCode = value;
                Settings.Default.LatestTrackingCode = _latestTrackingCode;
                Settings.Default.Save();
                TrackingCodeBox.Text = LatestTrackingCode;
            }
        }

        public StartWindow()
        {
            _keyboardHook = new KeyboardHook();

            Settings.Default.Upgrade();
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Initialize();

            _keyboardHook.KeyDown += keyboardHook_KeyDown;
            _keyboardHook.KeyUp += keyboardHook_KeyUp;

            //Installing the Keyboard Hooks
            _keyboardHook.Install();
        }

        private void keyboardHook_KeyDown(KeyboardHook.VKeys key)
        {
            if (key == KeyboardHook.VKeys.F2)
            {
                _isCommandPressed = true;
            }
            if (key == KeyboardHook.VKeys.F4 && _isCommandPressed)
            {
                _keyboardSim.Keyboard.TextEntry($"RR{TrackingCodeBox.Text}CZ");
                _keyboardSim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                _keyboardSim.Keyboard.Sleep(50);
                _keyboardSim.Keyboard.TextEntry(DateTime.Now.ToString("dd.MM.yyyy"));
                _keyboardSim.Keyboard.Sleep(50);
                _keyboardSim.Keyboard.ModifiedKeyStroke(new[] {VirtualKeyCode.SHIFT}, VirtualKeyCode.TAB);
                _keyboardSim.Keyboard.KeyPress(VirtualKeyCode.HOME);
                for (int i = 0; i < 8; i++)
                {
                    _keyboardSim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                }
                //MessageBox.Show("COMBO!");
            }
            //Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] KeyDown Event {" + key.ToString() + "}");
        }

        private void keyboardHook_KeyUp(KeyboardHook.VKeys key)
        {
            if (key == KeyboardHook.VKeys.F2)
            {
                _isCommandPressed = false;
            }
            //Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] KeyDown Event {" + key.ToString() + "}");
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled error: " + e.Exception.Message);
            MessageBox.Show("Unhandled error: " + e.Exception.StackTrace);
        }

        private void SelectAmazonReport()
        {
            var fromAmazonReports = GetParametersFromAmazonReport();
            if (fromAmazonReports == null) return;

            var source = new List<InvoiceXML.dataPackDataPackItem>();
            foreach (var report in fromAmazonReports)
            {
                var singleAmazonInvoice = FillDataPackItem(report, source.Count + 1);
                var existingDataPack = source.FirstOrDefault(di => di.invoice.invoiceHeader.symVar == singleAmazonInvoice.invoice.invoiceHeader.symVar);
                if (existingDataPack != null)
                { AddItemsToExistingDataPack(existingDataPack, singleAmazonInvoice); }
                else
                { source.Add(singleAmazonInvoice); }
            }

            var invoiceInvoiceItems = source.SelectMany(di => di.invoice.invoiceDetail);
            InvoiceItemsAll.Clear();
            foreach (var invoiceItem in invoiceInvoiceItems)
            {
                var itemWithDetails = new InvoiceItemWithDetails(invoiceItem, source.Single(di => (di.invoice.invoiceDetail).Contains(invoiceItem)).invoice.invoiceHeader);
                if (PackQuantityByItemName.ContainsKey(itemWithDetails.Item.text))
                {
                    itemWithDetails.PackQuantityMultiplier = int.Parse(PackQuantityByItemName[itemWithDetails.Item.text]);
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

            // same crazy stuff with discount - find a new and copy discount
            var discountItem1 = GetDiscountItem(list);
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

        private void Initialize()
        {
            _existingInvoceNumber = Settings.Default.ExistingInvoceNumber;
            _dphValue = Settings.Default.DPH;
            _defaultEmail = Settings.Default.DefaultEmail;
            _latestTrackingCode = Settings.Default.LatestTrackingCode;
            
            ExistingInvoiceNum.Text = ExistingInvoceNumber.ToString();
            DPHValue.Text = DPH.ToString();
            DefaultEmailBox.Text = DefaultEmail;
            TrackingCodeBox.Text = LatestTrackingCode;
            Rates = GetActualCurrencyRates();

            LoadSettings();
        }


        private Dictionary<string, decimal> GetActualCurrencyRates()
        {
            string downloadString;
            using (var webClient = new WebClient() { Encoding = XmlEncoding })
            {
                string str = DateTime.Today.AddDays(-1).ToString("dd.MM.yyyy");
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
            string city = FormatCity(valuesFromAmazon["ship-city"], valuesFromAmazon["ship-state"], string.Empty, headerSymPar);
            string fullAddress = FormatFullAddress(valuesFromAmazon["ship-address-1"], valuesFromAmazon["ship-address-2"], valuesFromAmazon["ship-address-3"], headerSymPar);
            string phoneNumber = FormatPhoneNumber(valuesFromAmazon["ship-phone-number"], valuesFromAmazon["buyer-phone-number"], headerSymPar);
            string shipCountry = valuesFromAmazon["ship-country"];
            string classification = SetClassification(shipCountry);
            string productName = FormatNameOfItem(valuesFromAmazon["product-name"]);
            string shipping = GetShippingTypeName(shipCountry, productName);

            string stockItemIds = GetItemCodeByName(productName);

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
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.mobilPhone = GetCustomsDeclaration(productName, classification);
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.email = DefaultEmail;
            packDataPackItem.invoice.invoiceHeader.paymentType.ids = salesChannel;
            packDataPackItem.invoice.invoiceHeader.centre.ids = GetSavedCenter(productName);
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

        private string GetCustomsDeclaration(string productName, string classification)
        {
            if (classification.EqualsIgnoreCase("UVzboží") && CustomsDeclarationByItemName.ContainsKey(productName))
            { return CustomsDeclarationByItemName[productName]; }
            return string.Empty;
        }

        private string GetShippingTypeName(string shipCountry, string productName)
        {
            string defaultShippingName = DefaultShippingNames.First();
            if (!_euContries.Contains(shipCountry))
            {
                return defaultShippingName;
            }

            if (!ShippingNameByItemName.ContainsKey(productName))
            {
                return defaultShippingName;
            }

            return ShippingNameByItemName[productName];
        }

        private string GetItemCodeByName(string productName)
        {
            string defaultCode = "----";
            if (!ProductCodeByItemName.ContainsKey(productName)) return defaultCode;

            return ProductCodeByItemName[productName];
        }

        private string GetSavedCenter(string productName)
        {
            string defaultCentreIds = "----";
            if (!ProductNumberByItemName.ContainsKey(productName)) return defaultCentreIds;

            return ProductNumberByItemName[productName];
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

            str = AskToChangeLongStringIfNeeded(message, str, MaxCityLength);

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
            return Rates.Single(r => r.Key.Equals(currency, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        private InvoiceXML.dataPackDataPackItem PrepareDatapackItem()
        {
            InvoiceXML.dataPack dataPack;
            using (StreamReader streamReader = new StreamReader("InvoiceBasic", XmlEncoding))
                dataPack = (InvoiceXML.dataPack)new XmlSerializer(typeof(InvoiceXML.dataPack)).Deserialize(streamReader);
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
            return (_euContries).Contains(shipCountry) ? "UDA5" : "UVzboží";
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

            var dictList = new List<Dictionary<string, string>>();
            foreach (var fileName in openFileDialog.FileNames)
            {
                var validLines = GetOrderDataLinesFromSingleFile(fileName, "\t");

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

        private static List<string[]> GetOrderDataLinesFromSingleFile(string fileName, string delimiter)
        {
            var lineItems = new List<string[]>();
            using (var textFieldParser = new TextFieldParser(fileName))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters(delimiter);
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

        private void DefaultEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            DefaultEmail = DefaultEmailBox.Text;
        }

        public void ButtonSelectInvoice_Click(object sender, RoutedEventArgs e)
        {
            SelectAmazonReport();
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            FixItems(InvoiceItemsAll);
            var convertedInvoices = _convertedInvoices;
            if (convertedInvoices != null && convertedInvoices.Any())
            {
                // disabled for now
                //foreach (var invoiceItemWithDetails in InvoiceItemsAll)
                //{
                //    if (invoiceItemWithDetails.Item.IsShipping
                //        && !DefaultShippingNames.Any(s => Equals(s.Trim(), invoiceItemWithDetails.Item.text.Trim())))
                //    {
                //        invoiceItemWithDetails.Header.carrier.ids = "Zásilkovna";
                //    }
                //}
                ExportToXML(_convertedInvoices);
            }
            else
            {
                MessageBox.Show("Zadne faktury nebyly konvertovany!");
            }
            SaveSettings();
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

        private void TopDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ProcessCustomChangedDataForProduct(e, 4, PackQuantityByItemName, (element) =>
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                string quantity = (e.EditingElement as TextBox).Text;
                if (!int.TryParse(quantity, out _)) throw new ArgumentException("Hodnota musi byt cele cislo! Prozatim aplikace pada...");
                return dataContextItem.Item.text; // TODO check if the number is integer !!
            });
            ProcessCustomChangedDataForProduct(e, 3, ProductCodeByItemName, (element) =>
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                return dataContextItem.Item.text;
            });
            ProcessCustomChangedDataForProduct(e, 2, ShippingNameByItemName, (element) =>
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                var symVar = dataContextItem.Header.symVar;

                // invoiceHeader is common for items in single Invoice, so it can be used for search
                var shippedItem = InvoiceItemsAll.FirstOrDefault(itemWithDetail =>
                    itemWithDetail.Header.symVar == symVar && itemWithDetail != dataContextItem);
                return shippedItem?.Item.text ?? string.Empty;
            });
        }

        private void BottomDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ProcessCustomChangedDataForProduct(e, 4, CustomsDeclarationByItemName, (element) =>
            {
                var dataContextItem = (InvoiceXML.dataPackDataPackItem)e.Row.DataContext;
                return dataContextItem.invoice.invoiceDetail.FirstOrDefault()?.text ?? string.Empty;
            });
            ProcessCustomChangedDataForProduct(e, 3, ProductNumberByItemName, (element) =>
            {
                var dataContextItem = (InvoiceXML.dataPackDataPackItem)e.Row.DataContext;
                return dataContextItem.invoice.invoiceDetail.FirstOrDefault()?.text ?? string.Empty;
            });
        }

        private void ProcessCustomChangedDataForProduct(
            DataGridCellEditEndingEventArgs e,
            int columnIndex,
            IDictionary<string, string> rememberedDictionary,
            Func<FrameworkElement, string> productNameGetter)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (!(e.Column is DataGridBoundColumn)) return;

                if (e.Column.DisplayIndex == columnIndex)
                {
                    string productName = productNameGetter(e.EditingElement);
                    string customValue = (e.EditingElement as TextBox).Text;
                    if (rememberedDictionary.ContainsKey(productName))
                    {
                        if (!rememberedDictionary[productName].Equals(customValue))
                            rememberedDictionary[productName] = customValue;
                    }
                    else
                    {
                        rememberedDictionary.Add(productName, customValue);
                    }
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _keyboardHook.KeyDown -= keyboardHook_KeyDown;
            _keyboardHook.KeyUp -= keyboardHook_KeyUp;
            _keyboardHook.Uninstall();
        }

        private void LoadSettings()
        {
            ProductCodeByItemName = DeserializeJsonDictionary(ProductCodeByItemNameJson);
            ProductNumberByItemName = DeserializeJsonDictionary(ProductNumberByItemNameJson);
            ShippingNameByItemName = DeserializeJsonDictionary(ShippingNameByItemNameJson);
            PackQuantityByItemName = DeserializeJsonDictionary(ProductQuantityByItemNameJson);
            CustomsDeclarationByItemName = DeserializeJsonDictionary(CustomsDeclarationByItemNameJson);
        }

        private void SaveSettings()
        {
            SerializeDictionaryToJson(ProductCodeByItemName, ProductCodeByItemNameJson);
            SerializeDictionaryToJson(ProductNumberByItemName, ProductNumberByItemNameJson);
            SerializeDictionaryToJson(ShippingNameByItemName, ShippingNameByItemNameJson);
            SerializeDictionaryToJson(PackQuantityByItemName, ProductQuantityByItemNameJson);
            SerializeDictionaryToJson(CustomsDeclarationByItemName, CustomsDeclarationByItemNameJson);
        }

        private Dictionary<string, string> DeserializeJsonDictionary(string fileName)
        {
            string json = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }

        private void SerializeDictionaryToJson(Dictionary<string, string> map, string fileName)
        {
            string json = JsonConvert.SerializeObject(map);
            File.WriteAllText(fileName, json);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectTransactions();
        }

        private void SelectTransactions()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Zvol soubor transakci",
                Filter = "Transakce|*.csv"
            };
            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == false) return;

            var reader = new TransactionsReader();

            var transactions = new List<Transaction>();
            foreach (var fileName in openFileDialog.FileNames)
            {
                transactions.AddRange(reader.ReadTransactions(fileName));
            }


            var saveFileDialog = new SaveFileDialog
            {
                Title = "Zvol umisteni vystupniho souboru",
                FileName = "Transactions_" + DateAndTime.Today.ToString("dd-MM-yyyy") + ".gpc"
            };
            bool? result = saveFileDialog.ShowDialog();
            if (result != true) return;

            var converter = new GpcGenerator();
            converter.SaveTransactions(transactions, saveFileDialog.FileName);
        }

        private void TrackingCodeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LatestTrackingCode = TrackingCodeBox.Text;
        }
    }
}
