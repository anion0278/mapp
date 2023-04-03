﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Xml;
using Castle.Core.Internal;
using CommunityToolkit.Mvvm.Input;
using Shmap.BusinessLogic.Invoices;
using Shmap.BusinessLogic.Invoices.Annotations;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Shmap.Models;
using Shmap.UI.Views.Resources;
using Shmap.ViewModels;
using Microsoft.Win32;
using Moq;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Shmap.UI.ViewModels
{
    internal class InvoiceConverterViewModel : ViewModelBase, IInvoiceConverterViewModel
    {
        private readonly IInvoiceConverter _invoiceConverter;
        private readonly IConfigProvider _configProvider;
        private readonly IFileOperationService _fileOperationService;
        private readonly ITransactionsReader _transactionsReader;
        private readonly IGpcGenerator _gpcGenerator;
        private readonly IAutocompleteData _autocompleteData;
        private readonly IDialogService _dialogService;
        private int _windowWidth;
        private int _windowHeight;
        private WindowState _windowState;
        private int _windowTop;
        private int _windowLeft;
        private uint? _existingInvoiceNumber;
        private string _defaultEmail;

        public RelayCommand SelectAmazonInvoicesCommand { get; }
        public RelayCommand ExportConvertedAmazonInvoicesCommand { get; }
        public RelayCommand ConvertTransactionsCommand { get; }
        public RelayCommand ConvertWarehouseDataCommand { get; set; }

        public ObservableCollection<InvoiceItemViewModel> InvoiceItems { get; } = new(); // TODO make private field?

        public ICollectionView InvoiceItemsCollectionView { get; }

        public BindingList<InvoiceViewModel> Invoices { get; } = new(); // use BindingList if it is required to update UI for inner items changes


        // TODO apply Fody

        public int WindowLeft // is it responsibility of the VM?
        {
            get => _windowLeft;
            set
            {
                SetWindowPositionConfig();
            }
        }

        public int WindowTop
        {
            get => _windowTop;
            set
            {
                SetWindowPositionConfig();
            }
        }

        public int WindowWidth
        {
            get => _windowWidth;
            set
            {
                SetWindowSizeConfig();
            }
        }

        public int WindowHeight
        {
            get => _windowHeight;
            set
            {
                SetWindowSizeConfig();
            }
        }

        public WindowState WindowState // TODO make windows agnostic
        {
            get => _windowState;
            set
            {
                _configProvider.IsMainWindowMaximized = value == WindowState.Maximized;
            }
        }

        [Required]
        public uint? ExistingInvoiceNumber
        {
            get => _existingInvoiceNumber;
            set
            {
                if (value.HasValue)
                {
                    _configProvider.ExistingInvoiceNumber = value.Value * 2; // TODO join methods, since names are same
                    _existingInvoiceNumber = value.Value;
                }
            }
        }

        [EmailAddress]
        public string DefaultEmail
        {
            get => _defaultEmail;
            set
            {
                value = value.ToLower();
                _defaultEmail = value;
                _configProvider.DefaultEmail = value; // TODO when value is not valid (according to the rules), it still saves data to config. Solve for all props
            }
        }

        public string TrackingCode
        {
            get => _configProvider.TrackingCode;
            set
            {
                _configProvider.TrackingCode = value;
            }
        }

        public bool OpenTargetFolderAfterConversion
        {
            get => _configProvider.OpenTargetFolderAfterConversion;
            set
            {
                _configProvider.OpenTargetFolderAfterConversion = value; // TODO move all config stuff to export part and use Fody PropertyChanged...
            }
        }

        public bool IsReadyForProcessing { get; private set; } = true; // I knwo that it does not make any sense, but Im just too tired

        public string WindowTitle { get; set; }


        public InvoiceConverterViewModel() // Design-time ctor
        {
            _windowHeight = 650;
            _windowWidth = 900;
            _existingInvoiceNumber = 123456789;
            _defaultEmail = "email@email.com";

            InvoiceItemsCollectionView = InitializeCollectionView();

            var invoice = new Invoice(new Dictionary<string, decimal>())
            {
                VariableSymbolFull = "203-5798943-2666737",
                ShipCountryCode = "GB",
                RelatedWarehouseName = "CGE",
                CustomsDeclaration = "1x deskova hra",
                SalesChannel = "amazon.com"
            };
            var items = new InvoiceItemBase[]
            {
                new InvoiceProduct(invoice)
                {
                    AmazonSku = "55-KOH-FR6885",
                    Name = "Dermacol Make-Up Cover, Waterproof Hypoallergenic for All Skin Types, Nr 218",
                    PackQuantityMultiplier = 1,
                    WarehouseCode = "CGE08",
                },
                new InvoiceItemGeneral(invoice, InvoiceItemType.Shipping)
                {
                    Name = "Shipping",
                },
                new InvoiceItemGeneral(invoice, InvoiceItemType.Discount)
                {
                    Name = "Discount"
                }
            };
            invoice.AddInvoiceItems(items);
            var dataMock = Mock.Of<IAutocompleteData>();
            foreach (var item in items)
            {
                InvoiceItems.Add(new InvoiceItemViewModel(item, dataMock)); // TODO create bindable collection with AddRange method
            }
            Invoices.Add(new InvoiceViewModel(invoice, dataMock));
        }

        public InvoiceConverterViewModel(IConfigProvider configProvider,
            IInvoiceConverter invoiceConverter,
            IFileOperationService fileOperationService,
            ITransactionsReader transactionsReader,
            IGpcGenerator gpcGenerator,
            IAutocompleteData autocompleteData,
            IDialogService dialogService)
        {
            _invoiceConverter = invoiceConverter;
            _configProvider = configProvider;
            _fileOperationService = fileOperationService;
            _transactionsReader = transactionsReader;
            _gpcGenerator = gpcGenerator;
            _autocompleteData = autocompleteData;
            _dialogService = dialogService;

            SelectAmazonInvoicesCommand = new RelayCommand(SelectAmazonInvoices, () => !HasErrors);
            ExportConvertedAmazonInvoicesCommand = new RelayCommand(ExportConvertedAmazonInvoices, ExportConvertedAmazonInvoicesCanExecute);
            ConvertTransactionsCommand = new RelayCommand(ConvertTransactions, () => true);
            ConvertWarehouseDataCommand = new RelayCommand(ConvertWarehouseData, () => true);

            InvoiceItemsCollectionView = InitializeCollectionView();

            WindowTitle += FormatTitleAssemblyFileVersion(Assembly.GetEntryAssembly());
            _windowHeight = _configProvider.MainWindowSize.Height;
            _windowWidth = _configProvider.MainWindowSize.Width;
            _windowState = _configProvider.IsMainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
            _windowLeft = _configProvider.MainWindowTopLeftCorner.X;
            _windowTop = _configProvider.MainWindowTopLeftCorner.Y;
            _existingInvoiceNumber = _configProvider.ExistingInvoiceNumber;
            _defaultEmail = _configProvider.DefaultEmail;
        }


        private async void ConvertWarehouseData()
        {
            IsReadyForProcessing = false;
            try
            {
                StockDataXmlSourceDefinition[] sources =
                {
                    new()
                    {
                        Url = "https://www.rappa.cz/export/vo.xml",
                        ItemNodeName = "SHOPITEM",
                        SkuNodeParsingOptions = new[]
                        {
                            new ValueParsingOption("EAN", null),
                        },
                        StockQuantityNodeParsingOptions = new[]
                        {
                            new ValueParsingOption("STOCK", null),
                        },
                    },
                    new()
                    {
                        Url = "https://en.bushman.eu/content/feeds/uQ5TueFNQh_expando_4.xml",
                        ItemNodeName = "item",
                        SkuNodeParsingOptions = new[]
                        {
                            new ValueParsingOption("g:gtin", null),
                            new ValueParsingOption("g:sku_with_ean", @"\d{13}"),
                        },
                        StockQuantityNodeParsingOptions = new[]
                        {
                            new ValueParsingOption("g:quantity", null),
                        },
                    }
                };

                var stockQuantityUpdater = new StockQuantityUpdater();
                var stockData = await stockQuantityUpdater.ConvertWarehouseData(sources);
                var columnNamesLine =
                    "sku\tprice\tminimum-seller-allowed-price\tmaximum-seller-allowed-price\tquantity\thandling-time\tfulfillment-channel";
                var lines = new List<string>(stockData.Count() + 1) { columnNamesLine };
                string lineTemplate = "{0}\t\t\t\t{1}\t\t";
                foreach (var stockInfo in stockData)
                {
                    lines.Add(lineTemplate.FormatWith(stockInfo.Sku, stockInfo.Quantity));
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Zvol umisteni vystupniho souboru",
                    FileName = "stockQuantity_" + DateTime.Today.ToString("dd-MM-yyyy") + ".txt",
                    Filter = "Text files|*.txt"
                };
                bool? result = saveFileDialog.ShowDialog();
                if (result != true) return;

                await File.WriteAllLinesAsync(saveFileDialog.FileName, lines);
                if (_configProvider.OpenTargetFolderAfterConversion)
                {
                    _fileOperationService.OpenFileFolder(saveFileDialog.FileName);
                }
            }
            finally { IsReadyForProcessing = true; }
        }


        private ICollectionView InitializeCollectionView()
        {
            var collectionView = GetNewCollectionViewInstance(InvoiceItems);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(InvoiceItemViewModel.AmazonNumber)));
            return collectionView;
        }

        private bool ExportConvertedAmazonInvoicesCanExecute()
        {
            return !HasErrors && InvoiceItems.Any() && Invoices.Any()
                   && InvoiceItems.All(i => !i.HasErrors)
                   && Invoices.All(i => !i.HasErrors);
        }

        private string FormatTitleAssemblyFileVersion(Assembly assembly)
        {
            try
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
                return "Mapp v" + new Version(fileVersion.FileVersion).ToString(3);
            }
            catch (Exception)
            {
                return "Mapp v" + assembly.GetName().Version.ToString(3);
            }
        }

        private void ConvertTransactions()
        {
            var fileNames = _fileOperationService.GetTransactionFileNames();
            if (!fileNames.Any()) return;

            var transactions = _transactionsReader.ReadTransactionsFromMultipleFiles(fileNames);

            string saveFileName = _fileOperationService.GetSaveFileNameForConvertedTransactions();
            if (string.IsNullOrWhiteSpace(saveFileName)) return;

            _gpcGenerator.SaveTransactions(transactions, saveFileName);

            if (_configProvider.OpenTargetFolderAfterConversion)
            {
                _fileOperationService.OpenFileFolder(saveFileName);
            }
        }

        private void SelectAmazonInvoices()
        {
            InvoiceItems.Clear();
            Invoices.Clear();

            var fileNames = _fileOperationService.OpenAmazonInvoices();
            if (!fileNames.Any()) return;

            var conversionContext = new InvoiceConversionContext() // TODO injected factory
            {
                ConvertToDate = DateTime.Today,
                DefaultEmail = DefaultEmail, // TODO decide whether to take it from config or from VM
                ExistingInvoiceNumber = ExistingInvoiceNumber.Value,
            };
            var invoices = _invoiceConverter.LoadAmazonReports(fileNames, conversionContext).ToList();

            foreach (var invoiceItem in invoices.SelectMany(di => di.InvoiceItems))
            {
                InvoiceItems.Add(new InvoiceItemViewModel(invoiceItem, _autocompleteData));
            }

            foreach (var invoice in invoices)
            {
                Invoices.Add(new InvoiceViewModel(invoice, _autocompleteData));
            }
        }

        private void ExportConvertedAmazonInvoices()
        {
            string fileName = _fileOperationService.SaveConvertedAmazonInvoices();
            if (string.IsNullOrWhiteSpace(fileName)) return;

            var invoices = Invoices.Select(i => i.ExportModel()).ToList();
            
            // TODO how to avoid need for calling ToList (due to laziness of linq)?
            var invoiceItems = InvoiceItems.Select(i => i.ExportModel()).ToList();

            if (!invoices.Any())
            {
                _dialogService.ShowMessage("Zadne faktury nebyly konvertovany!"); // TODO solve using OperationResult
                return;
            }

            _invoiceConverter.ProcessInvoices(invoices, fileName);
            ExistingInvoiceNumber += (uint)invoices.Count;

            if (_configProvider.OpenTargetFolderAfterConversion)
            {
                _fileOperationService.OpenFileFolder(fileName);
            }
        }

        private void SetWindowSizeConfig()
        {
            _configProvider.MainWindowSize = new Size(WindowWidth, WindowHeight);
            _configProvider.SaveConfig();
        }

        private void SetWindowPositionConfig()
        {
            _configProvider.MainWindowTopLeftCorner = new Point(_windowLeft, _windowTop);
            _configProvider.SaveConfig();
        }
    }
}