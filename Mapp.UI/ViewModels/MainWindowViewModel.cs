using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Invoices;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Shmap.ViewModels
{
    public interface IMainWindowViewModel
    {
        RelayCommand SelectAmazonInvoicesCommand { get; }
        RelayCommand ExportConvertedAmazonInvoicesCommand { get; }
        RelayCommand ConvertTransactionsCommand { get; }
        int WindowLeft { get; set; }
        int WindowTop { get; set; }
        int WindowWidth { get; set; }
        int WindowHeight { get; set; }
        WindowState WindowState { get; set; }
        uint? ExistingInvoiceNumber { get; set; }
        string DefaultEmail { get; set; }
        string LatestTrackingCode { get; set; }
        bool OpenTargetFolderAfterConversion { get; set; }
        public ObservableCollection<InvoiceItemWithDetailViewModel> InvoiceItems { get; set; }
        public ObservableCollection<InvoiceViewModel> Invoices { get; set; }
    }

    internal class MainWindowViewModel : ViewModelWithErrorValidationBase, IMainWindowViewModel
    {
        public IInvoiceConverter _invoiceConverter;
        private readonly IConfigProvider _configProvider;
        private readonly IAutoKeyboardInputHelper _autoKeyboardInputHelper;
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
        private string _latestTrackingCode;
        private bool _openTargetFolderAfterConversion;
        private string _windowTitle;
        private ObservableCollection<InvoiceItemWithDetailViewModel> _invoiceItems = new();
        private ObservableCollection<InvoiceViewModel> _invoices = new();

        public RelayCommand SelectAmazonInvoicesCommand { get; }
        public RelayCommand ExportConvertedAmazonInvoicesCommand { get; }
        public RelayCommand ConvertTransactionsCommand { get; }
        public RelayCommand WindowClosingCommand { get; }

        public ObservableCollection<InvoiceItemWithDetailViewModel> InvoiceItems
        {
            get => _invoiceItems;
            set => Set(ref _invoiceItems, value);
        }

        public ObservableCollection<InvoiceViewModel> Invoices
        {
            get => _invoices;
            set => Set(ref _invoices, value);
        }

        public int WindowLeft
        {
            get { return _windowLeft; }
            set
            {
                Set(ref _windowLeft, value);
                SetWindowPositionConfig();
            }
        }

        public int WindowTop
        {
            get { return _windowTop; }
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

        public WindowState WindowState
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
            get { return _defaultEmail; }
            set
            {
                value = value.ToLower();
                Set(ref _defaultEmail, value);
                _configProvider.DefaultEmail = value;
            }
        }

        public string LatestTrackingCode
        {
            get => _configProvider.LatestTrackingCode;
            set
            {
                Set(ref _latestTrackingCode, value);
                _configProvider.LatestTrackingCode = value;
                _autoKeyboardInputHelper.TrackingCode = value; // TODO SOLVE in a better way
            }
        }

        public bool OpenTargetFolderAfterConversion
        {
            get => _configProvider.OpenTargetFolderAfterConversion;
            set
            {
                Set(ref _openTargetFolderAfterConversion, value);
                _configProvider.OpenTargetFolderAfterConversion = value;
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => Set(ref _windowTitle, value);
        }

        public MainWindowViewModel() // Design-time ctor
        {
            _windowHeight = 600;
            _windowWidth = 900;
            _existingInvoiceNumber = 123456789;
            _defaultEmail = "email@email.com";
        }

        public MainWindowViewModel(IConfigProvider configProvider,
            IInvoiceConverter invoiceConverter,
            IAutoKeyboardInputHelper autoKeyboardInputHelper,
            IFileOperationService fileOperationService,
            ITransactionsReader transactionsReader,
            IGpcGenerator gpcGenerator,
            IAutocompleteData autocompleteData,
            IDialogService dialogService)
        {
            _invoiceConverter = invoiceConverter; 
            _configProvider = configProvider;
            _autoKeyboardInputHelper = autoKeyboardInputHelper;
            _fileOperationService = fileOperationService;
            _transactionsReader = transactionsReader;
            _gpcGenerator = gpcGenerator;
            _autocompleteData = autocompleteData;
            _dialogService = dialogService;
            SelectAmazonInvoicesCommand = new RelayCommand(SelectAmazonInvoices, () => !HasErrors);
            ExportConvertedAmazonInvoicesCommand = new RelayCommand(ExportConvertedAmazonInvoices, () => InvoiceItems.Any() && Invoices.Any());
            ConvertTransactionsCommand = new RelayCommand(ConvertTransactions, () => true);
            WindowClosingCommand = new RelayCommand(OnWindowClosing, () => true);

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

            _latestTrackingCode = _configProvider.LatestTrackingCode;
            _autoKeyboardInputHelper.TrackingCode = _configProvider.LatestTrackingCode; // TODO Really bad
        }

        private void OnWindowClosing()
        {
            _autoKeyboardInputHelper.Dispose();
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
                InvoiceItems.Add(new InvoiceItemWithDetailViewModel(invoiceItem, _autocompleteData));
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