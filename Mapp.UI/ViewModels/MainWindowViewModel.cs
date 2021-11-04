using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight.CommandWpf;
using Shmap.BusinessLogic.Invoices;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Shmap.Models;
using Shmap.ViewModels;
using Moq;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Shmap.UI.ViewModels
{
    internal class MainWindowViewModel : ViewModelWithErrorValidationBase, IMainWindowViewModel
    {
        public IInvoiceConverter _invoiceConverter;
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
        private string _trackingCode;
        private bool _openTargetFolderAfterConversion;
        private string _windowTitle;

        public RelayCommand SelectAmazonInvoicesCommand { get; }
        public RelayCommand ExportConvertedAmazonInvoicesCommand { get; }
        public RelayCommand ConvertTransactionsCommand { get; }
        public RelayCommand WindowClosingCommand { get; }

        public ObservableCollection<InvoiceItemViewModel> InvoiceItems { get; } = new(); // TODO make private field?

        public ICollectionView InvoiceItemsCollectionView { get; }

        public ObservableCollection<InvoiceViewModel> Invoices { get; } = new();

        // TODO apply Fody
        public int WindowLeft // is it responsibility of the VM?
        {
            get => _windowLeft;
            set
            {
                Set(ref _windowLeft, value);
                SetWindowPositionConfig();
            }
        }

        public int WindowTop
        {
            get => _windowTop;
            set
            {
                Set(ref _windowTop, value);
                SetWindowPositionConfig();
            }
        }

        public int WindowWidth
        {
            get => _windowWidth;
            set
            {
                Set(ref _windowWidth, value);
                SetWindowSizeConfig();
            }
        }

        public int WindowHeight
        {
            get => _windowHeight;
            set
            {
                Set(ref _windowHeight, value);
                SetWindowSizeConfig();
            }
        }

        public WindowState WindowState // TODO make windows agnostic
        {
            get => _windowState;
            set
            {
                Set(ref _windowState, value);
                _configProvider.IsMainWindowMaximized = value == WindowState.Maximized;
            }
        }

        public uint? ExistingInvoiceNumber
        {
            get => _existingInvoiceNumber;
            set
            {
                Set(ref _existingInvoiceNumber, value);
                if (value.HasValue) _configProvider.ExistingInvoiceNumber = value.Value; // TODO join methods, since names are same
            }
        }

        public string DefaultEmail
        {
            get => _defaultEmail;
            set
            {
                value = value.ToLower();
                Set(ref _defaultEmail, value);
                _configProvider.DefaultEmail = value; // TODO when value is not valid (according to the rules), it still saves data to config. Solve for all props
            }
        }

        public string TrackingCode
        {
            get => _configProvider.TrackingCode;
            set
            {
                Set(ref _trackingCode, value);
                _configProvider.TrackingCode = value;
            }
        }

        public bool OpenTargetFolderAfterConversion
        {
            get => _configProvider.OpenTargetFolderAfterConversion;
            set
            {
                Set(ref _openTargetFolderAfterConversion, value);
                _configProvider.OpenTargetFolderAfterConversion = value; // TODO move all config stuff to export part and use Fody PropertyChanged...
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => Set(ref _windowTitle, value);
        }

        public MainWindowViewModel() // Design-time ctor
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

        public MainWindowViewModel(IConfigProvider configProvider,
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
            WindowClosingCommand = new RelayCommand(OnWindowClosing, () => true);

            InvoiceItemsCollectionView = InitializeCollectionView();

            AddValidationRule(() => DefaultEmail, () => MailAddress.TryCreate(DefaultEmail, out _), "Zadany email neni v poradku");
            AddValidationRule(() => ExistingInvoiceNumber, () => ExistingInvoiceNumber.HasValue, "Cislo faktury neni zadano spravne");

            _windowTitle += FormatTitleAssemblyFileVersion(Assembly.GetEntryAssembly());
            _windowHeight = _configProvider.MainWindowSize.Height;
            _windowWidth = _configProvider.MainWindowSize.Width;
            _windowState = _configProvider.IsMainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
            _windowLeft = _configProvider.MainWindowTopLeftCorner.X;
            _windowTop = _configProvider.MainWindowTopLeftCorner.Y;
            _existingInvoiceNumber = _configProvider.ExistingInvoiceNumber;
            _defaultEmail = _configProvider.DefaultEmail;
            _trackingCode = _configProvider.TrackingCode;

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

        private void OnWindowClosing()
        { }

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
            var invoiceItems = InvoiceItems.Select(i => i.ExportModel()).ToList();
            // TODO how to avoid need for calling ToList (due to laziness of linq)?

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