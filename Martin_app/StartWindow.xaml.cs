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
        public IInvoiceConverter InvoiceConverter { get; }
        private IAutocompleteData _autocompleteData;
        private AutocompleteDataLoader _autocompleteDataLoader;
        private TransactionsReader _transactionsReader;
        private readonly IConfigProvider _currentConfig;
        private StartWindowViewModel _startWindowViewModel;
        private IFileOperationService _fileOperationService;

        public StartWindow()
        {
            var settings = AppSettings.Default;
            settings.UpgradeSettingsIfRequired();
            _currentConfig = new ConfigProvider(settings, true);

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

            Title += FormatTitleAssemblyFileVersion(Assembly.GetEntryAssembly());

            _startWindowViewModel = new StartWindowViewModel(_currentConfig, InvoiceConverter, _autoKeyboardInputHelper);
            DataContext = _startWindowViewModel;

            _appUpdater.CheckUpdate();
            _fileOperationService = new FileOperationsService();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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
        }

        public void ButtonSelectInvoice_Click(object sender, RoutedEventArgs e)
        {
            var fileNames = _fileOperationService.OpenAmazonInvoices();
            if (!fileNames.Any()) return;

            var conversionContext = new InvoiceConversionContext()
            {
                ConvertToDate = DateTime.Today,
                DefaultEmail = _currentConfig.DefaultEmail, // TODO decide whether to take it from config or from VM
                ExistingInvoiceNumber = _currentConfig.ExistingInvoiceNumber,
            };
            InvoiceConverter.LoadAmazonReports(fileNames, conversionContext);
        }
         
        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            string fileName = _fileOperationService.SaveConvertedAmazonInvoices();
            if (string.IsNullOrWhiteSpace(fileName)) return;

            InvoiceConverter.ProcessInvoices(fileName, out uint processedInvoices);
            _startWindowViewModel.ExistingInvoiceNumber += processedInvoices;

            if (_currentConfig.OpenTargetFolderAfterConversion)
            {
                _fileOperationService.OpenFileFolder(fileName);
            }
        }

        private void TransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            var fileNames = _fileOperationService.GetTransactionFileNames();
            if (!fileNames.Any()) return;

            var transactions = _transactionsReader.ReadTransactionsFromMultipleFiles(fileNames);

            string saveFileName = _fileOperationService.GetSaveFileNameForConvertedTransactions();
            if (string.IsNullOrWhiteSpace(saveFileName)) return;

            var converter = new GpcGenerator();
            converter.SaveTransactions(transactions, saveFileName);

            if (_currentConfig.OpenTargetFolderAfterConversion)
            {
                _fileOperationService.OpenFileFolder(saveFileName);
            }
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
    }
}
