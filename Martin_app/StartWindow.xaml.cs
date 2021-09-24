using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Currency;
using Shmap.BusinessLogic.Invoices;
using Shmap.DataAccess;
using Mapp;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Microsoft.Win32;


namespace Mapp
{
    public partial class StartWindow : Window
    {
        private AutoKeyboardInputHelper _autoKeyboardInputHelper;
        private ApplicationUpdater.ApplicationUpdater _appUpdater;
        private IJsonManager _jsonManager;
        private IInvoicesXmlManager _invoiceXmlXmlManager;
        private CsvLoader _csvLoader;
        public InvoiceConverter InvoiceConverter { get; }
        private IAutocompleteData _autocompleteData;
        private AutocompleteDataLoader _autocompleteDataLoader;
        private TransactionsReader _transactionsReader;
        private AppSettings _currentSettings;

        public StartWindow()
        {
            _currentSettings = AppSettings.Default;
            _currentSettings.UpgradeSettingsIfRequired();

            var vm = new StartWindowViewModel(_currentSettings);
            DataContext = vm;


            string invoiceDir = "Invoice Converter";
            // TODO IOC container!!
            _jsonManager = new JsonManager();
            _autoKeyboardInputHelper = new AutoKeyboardInputHelper();
            _appUpdater = new ApplicationUpdater.ApplicationUpdater(_jsonManager) { UserNotification = (o, e) => MessageBox.Show(e) };
            _invoiceXmlXmlManager = new InvoicesXmlManager(invoiceDir){ UserNotification = (o, e) => MessageBox.Show(e) };
            _csvLoader = new CsvLoader(invoiceDir);
            _autocompleteDataLoader = new AutocompleteDataLoader(_jsonManager, invoiceDir);
            _autocompleteData = _autocompleteDataLoader.LoadSettings();
            InvoiceConverter = new InvoiceConverter(_autocompleteData, new CurrencyConverter(), _csvLoader, _invoiceXmlXmlManager, AskToChangeLongStringIfNeeded, _autocompleteDataLoader);
            _transactionsReader = new TransactionsReader(_jsonManager);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            

            //TODO pass as argument
            InvoiceConverter.ExistingInvoiceNumber = _currentSettings.ExistingInvoiceNumber;
            InvoiceConverter.CountryVat = _currentSettings.DPH;
            InvoiceConverter.DefaultEmail = _currentSettings.DefaultEmail;
            InvoiceConverter.LatestTrackingCode = _currentSettings.LatestTrackingCode; // TODO - move to correct assembly!
            
            UpdateTextBoxes();

            Title += FormatTitleAssemblyFileVersion(Assembly.GetEntryAssembly());

            _appUpdater.CheckUpdate();
        }


        private void UpdateTextBoxes() //not mvvm at all for now
        {
            ExistingInvoiceNum.Text = InvoiceConverter.ExistingInvoiceNumber.ToString();
            DefaultEmailBox.Text = InvoiceConverter.DefaultEmail;
            TrackingCodeBox.Text = InvoiceConverter.LatestTrackingCode;
        }

        private string FormatTitleAssemblyFileVersion(Assembly assembly) // TODO shared assembly info file?
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return " v" + fileVersion.FileVersion;
        }


        private void TopDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ProcessCustomChangedDataForProduct(e, 5, _autocompleteData.PackQuantitySku, (element) => // TODO its VERY BAD to rely on column NUMBER
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                string quantity = (e.EditingElement as TextBox).Text;
                if (!int.TryParse(quantity, out _)) throw new ArgumentException("Hodnota musi byt cele cislo! Prozatim aplikace pada...");
                return dataContextItem.Item.amazonSkuCode; // TODO check if the number is integer !!
            });
            ProcessCustomChangedDataForProduct(e, 4, _autocompleteData.PohodaProdCodeBySku, (element) =>
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                return dataContextItem.Item.amazonSkuCode;
            });
            ProcessCustomChangedDataForProduct(e, 2, _autocompleteData.ShippingNameBySku, (element) =>
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                var symVar = dataContextItem.Header.symVar;

                // invoiceHeader is common for items in single Invoice, so it can be used for search
                var shippedItem = InvoiceConverter.InvoiceItemsAll.FirstOrDefault(itemWithDetail =>
                    itemWithDetail.Header.symVar == symVar && itemWithDetail != dataContextItem);
                return shippedItem?.Item.amazonSkuCode ?? string.Empty; // DRY principle for Item.amazonSkuCode
            });
        }

        private void BottomDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // for both CustomDeclarations and Warehouse code we use the first Item's SKU to remember settings
            ProcessCustomChangedDataForProduct(e, 5, _autocompleteData.CustomsDeclarationBySku, (_) =>
            {
                var dataContextItem = (InvoiceXml.dataPackDataPackItem)e.Row.DataContext;
                return dataContextItem.invoice.invoiceDetail.FirstOrDefault()?.amazonSkuCode ?? string.Empty;
            });
            ProcessCustomChangedDataForProduct(e, 4, _autocompleteData.ProdWarehouseSectionBySku, (_) =>
            {
                var dataContextItem = (InvoiceXml.dataPackDataPackItem)e.Row.DataContext;
                return dataContextItem.invoice.invoiceDetail.FirstOrDefault()?.amazonSkuCode ?? string.Empty;
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
                    string productSku = productNameGetter(e.EditingElement); 
                    string customValue = (e.EditingElement as TextBox).Text;
                    if (productSku != null && customValue != ApplicationConstants.EmptyItemCode && !string.IsNullOrEmpty(customValue)) // TODO - fix. VERY BAD
                    {
                        if (rememberedDictionary.ContainsKey(productSku))
                        {
                            if (!rememberedDictionary[productSku].Equals(customValue))
                                rememberedDictionary[productSku] = customValue;
                        }
                        else
                        {
                            rememberedDictionary.Add(productSku, customValue);
                        }
                    }
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _autoKeyboardInputHelper.Dispose();

            _currentSettings.ExistingInvoiceNumber = InvoiceConverter.ExistingInvoiceNumber;
            _currentSettings.DPH = InvoiceConverter.CountryVat;
            _currentSettings.DefaultEmail = InvoiceConverter.DefaultEmail;
            _currentSettings.LatestTrackingCode= InvoiceConverter.LatestTrackingCode;
            _currentSettings.Save();
        }

        
        private void ExistingInvoiceNum_LostFocus(object sender, RoutedEventArgs e)
        {
            uint existingInvoceNumber;
            try
            {
                existingInvoceNumber = uint.Parse(_csvLoader.ToInvariantFormat(ExistingInvoiceNum.Text));
            }
            catch (Exception ex)
            {
                ButtonConvert.IsEnabled = false;
                MessageBox.Show("Invoice number je zadan spatne!");
                return;
            }
            ButtonConvert.IsEnabled = true;
            InvoiceConverter.ExistingInvoiceNumber = existingInvoceNumber;
        }

        private void DefaultEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            InvoiceConverter.DefaultEmail = DefaultEmailBox.Text;
        }

        public void ButtonSelectInvoice_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Zvol Amazon report",
                Filter = "Amazon Report|*.txt"
            };
            bool? dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == false) return;

            InvoiceConverter.LoadAmazonReports(openFileDialog.FileNames, DateTime.Today);
        }
         
        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Zvol vystupni slozku",
                FileName = "PohodaInvoices_" + DateTime.Today.ToString("dd-MM-yyyy") + ".xml",
                Filter = "Converted Amazon Report|*.xml"
            };
            bool? dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult != true) return;

            InvoiceConverter.ProcessInvoices(saveFileDialog.FileName);
            OpenFileFolder(saveFileDialog.FileName);
            UpdateTextBoxes();
        }

        private void OpenFileFolder(string fileName)
        {
            Process.Start("explorer.exe", Path.GetDirectoryName(fileName));
        }

        private void TransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Zvol soubor transakci",
                Filter = "Transactions|*.csv"
            };
            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == false) return;

            var transactions = _transactionsReader.ReadTransactionsFromMultipleFiles(openFileDialog.FileNames);

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Zvol umisteni vystupniho souboru",
                FileName = "Transactions_" + DateTime.Today.ToString("dd-MM-yyyy") + ".gpc",
                Filter = "Converted Transactions|*.gpc"
            };
            bool? result = saveFileDialog.ShowDialog();
            if (result != true) return;

            var converter = new GpcGenerator();
            converter.SaveTransactions(transactions, saveFileDialog.FileName);
            OpenFileFolder(saveFileDialog.FileName);
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


        private void TrackingCodeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InvoiceConverter.LatestTrackingCode = TrackingCodeBox.Text;
        }


        private void TrakingCodeBox_ChangedText(object sender, TextChangedEventArgs e)
        {
            _autoKeyboardInputHelper.TrackingCode = TrackingCodeBox.Text;
        }
    }
}
