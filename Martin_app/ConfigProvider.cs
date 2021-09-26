using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Mapp
{
    internal interface IConfigProvider
    {
        uint ExistingInvoiceNumber { get; set; }
        string DefaultEmail { get; set; }
        string LatestTrackingCode { get; set; }
        bool IsMainWindowMaximized { get; set; }
        Size MainWindowSize { get; set; }
        Point MainWindowTopLeftCorner { get; set; }
        bool OpenTargetFolderAfterConversion { get; set; }
        void SaveConfig();
    }

    public class ConfigProvider: IConfigProvider
    {
        private readonly AppSettings _settings;
        private readonly bool _isAutosaveEnabled;

        public bool IsMainWindowMaximized
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public Size MainWindowSize
        {
            get => GetValue<Size>();
            set => SetValue(value);
        }

        public Point MainWindowTopLeftCorner
        {
            get => GetValue<Point>();
            set => SetValue(value);
        }

        public uint ExistingInvoiceNumber
        {
            get => GetValue<uint>();
            set => SetValue(value);
        }

        public string DefaultEmail
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string LatestTrackingCode
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool OpenTargetFolderAfterConversion
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        private void SetValue<T>(T value, [CallerMemberName] string propertyName = "")
        {
            _settings[propertyName] = value;
            if (_isAutosaveEnabled)
            {
                SaveConfig();
            }
        }

        private T GetValue<T>([CallerMemberName] string propertyName = "")
        {
            return (T)_settings[propertyName];
        }


        public ConfigProvider(AppSettings settings, bool isAutosaveEnabled)
        {
            _settings = settings;
            _isAutosaveEnabled = isAutosaveEnabled;
        }

        public void SaveConfig()
        {
            _settings.Save();
        }
    }
}