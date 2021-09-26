
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Invoices;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Mapp
{
    internal interface IStartWindowViewModel
    {
        IInvoiceConverter InvoiceConverter { get; }
        RelayCommand SelectAmazonInvoicesCommand { get; }
        RelayCommand ExportConvertedAmazonInvoicesCommand { get; }
        RelayCommand ConvertTransactionsCommand { get; }
        int WindowLeft { get; set; }
        int WindowTop { get; set; }
        int WindowWidth { get; set; }
        int WindowHeight { get; set; }
        WindowState WindowState { get; set; }
        uint ExistingInvoiceNumber { get; set; }
        string DefaultEmail { get; set; }
        string LatestTrackingCode { get; set; }
        bool OpenTargetFolderAfterConversion { get; set; }
    }

    internal class StartWindowViewModel: ViewModelBase, IStartWindowViewModel
    {
        public IInvoiceConverter InvoiceConverter { get; }
        private readonly IConfigProvider _configProvider;
        private readonly IAutoKeyboardInputHelper _autoKeyboardInputHelper;
        private readonly IFileOperationService _fileOperationService;
        private readonly ITransactionsReader _transactionsReader;
        private readonly IGpcGenerator _gpcGenerator;
        private int _windowWidth;
        private int _windowHeight;
        private WindowState _windowState;
        private int _windowTop;
        private int _windowLeft;
        private uint _existingInvoiceNumber;
        private string _defaultEmail;
        private string _latestTrackingCode;
        private bool _openTargetFolderAfterConversion;
        private string _windowTitle;

        public RelayCommand SelectAmazonInvoicesCommand { get; private set; }
        public RelayCommand ExportConvertedAmazonInvoicesCommand { get; private set; }
        public RelayCommand ConvertTransactionsCommand { get; private set; }

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

        public uint ExistingInvoiceNumber
        {
            get => _existingInvoiceNumber;
            set
            {
                Set(ref _existingInvoiceNumber, value);
                _configProvider.ExistingInvoiceNumber = value; // TODO join methods, since names are same
            }
        }

        public string DefaultEmail
        {
            get { return _defaultEmail; }
            set
            {
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


        public StartWindowViewModel(IConfigProvider configProvider,
            IInvoiceConverter invoiceConverter,
            IAutoKeyboardInputHelper autoKeyboardInputHelper,
            IFileOperationService fileOperationService,
            ITransactionsReader transactionsReader, 
            IGpcGenerator gpcGenerator)
        {
            InvoiceConverter = invoiceConverter; // TODO FIXME
            _configProvider = configProvider;
            _autoKeyboardInputHelper = autoKeyboardInputHelper;
            _fileOperationService = fileOperationService;
            _transactionsReader = transactionsReader;
            _gpcGenerator = gpcGenerator;
            SelectAmazonInvoicesCommand = new RelayCommand(SelectAmazonInvoices, ()=>true);
            ExportConvertedAmazonInvoicesCommand = new RelayCommand(ExportConvertedAmazonInvoices, ()=>true);
            ConvertTransactionsCommand = new RelayCommand(ConvertTransactions, ()=>true);

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

        private string FormatTitleAssemblyFileVersion(Assembly assembly) // TODO shared assembly info file?
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return " v" + fileVersion.FileVersion;
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
            var fileNames = _fileOperationService.OpenAmazonInvoices();
            if (!fileNames.Any()) return;

            var conversionContext = new InvoiceConversionContext()
            {
                ConvertToDate = DateTime.Today,
                DefaultEmail = _configProvider.DefaultEmail, // TODO decide whether to take it from config or from VM
                ExistingInvoiceNumber = _configProvider.ExistingInvoiceNumber,
            };
            InvoiceConverter.LoadAmazonReports(fileNames, conversionContext);
        }

        private void ExportConvertedAmazonInvoices()
        {
            string fileName = _fileOperationService.SaveConvertedAmazonInvoices();
            if (string.IsNullOrWhiteSpace(fileName)) return;

            InvoiceConverter.ProcessInvoices(fileName, out uint processedInvoices);
            ExistingInvoiceNumber += processedInvoices;

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