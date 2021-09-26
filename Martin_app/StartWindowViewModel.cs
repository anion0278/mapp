
using System.Drawing;
using System.Windows;
using GalaSoft.MvvmLight;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Invoices;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Mapp
{
    internal class StartWindowViewModel: ViewModelBase
    {
        public IInvoiceConverter InvoiceConverter { get; }
        private readonly IConfigProvider _configProvider;
        private readonly IAutoKeyboardInputHelper _autoKeyboardInputHelper;
        private int _windowWidth;
        private int _windowHeight;
        private WindowState _windowState;
        private int _windowTop;
        private int _windowLeft;
        private uint _existingInvoiceNumber;
        private string _defaultEmail;
        private string _latestTrackingCode;
        private bool _openTargetFolderAfterConversion;

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


        public StartWindowViewModel(IConfigProvider configProvider, IInvoiceConverter invoiceConverter, IAutoKeyboardInputHelper autoKeyboardInputHelper)
        {
            InvoiceConverter = invoiceConverter; // TODO FIXME
            _configProvider = configProvider;
            _autoKeyboardInputHelper = autoKeyboardInputHelper;

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