
using System.Drawing;
using System.Windows;
using GalaSoft.MvvmLight;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Mapp
{
    public class StartWindowViewModel: ViewModelBase
    {
        private readonly AppSettings _settings;
        private int _windowWidth;
        private int _windowHeight;
        private WindowState _windowState;
        private int _windowTop;
        private int _windowLeft;

        public int WindowLeft
        {
            get { return _windowLeft; }
            set
            {
                Set(ref _windowLeft, value);
                SaveWindowPosition();
            }
        }


        public int WindowTop
        {
            get { return _windowTop; }
            set
            {
                Set(ref _windowTop, value);
                SaveWindowPosition();
            }
        }

        public int WindowWidth
        {
            get => _windowWidth;
            set
            {
                Set(ref _windowWidth, value);
                SaveWindowSize();
            }
        }

        public int WindowHeight
        {
            get => _windowHeight;
            set
            {
                Set(ref _windowHeight, value);
                SaveWindowSize();
            }
        }


        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                Set(ref _windowState, value);
                _settings.IsMainWindowMaximized = value == WindowState.Maximized;
                _settings.Save();
            }
        }

        public StartWindowViewModel(AppSettings settings)
        {
            _settings = settings;
            _windowHeight = _settings.MainWindowSize.Height;
            _windowWidth = _settings.MainWindowSize.Width;
            _windowState = _settings.IsMainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
            _windowLeft = _settings.MainWindowTopLeftCorner.X;
            _windowTop = _settings.MainWindowTopLeftCorner.Y;
        }

        private void SaveWindowSize()
        {
            _settings.MainWindowSize = new Size(WindowWidth, WindowHeight);
            _settings.Save();
        }

        private void SaveWindowPosition()
        {
            _settings.MainWindowTopLeftCorner = new Point(_windowLeft, _windowTop);
            _settings.Save();
        }
    }
}