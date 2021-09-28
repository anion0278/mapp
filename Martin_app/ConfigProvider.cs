﻿using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Shmap.CommonServices;
using Shmap.DataAccess;

namespace Mapp
{
  
    public class ConfigProvider: IConfigProvider
    {
        private readonly AppSettings _settings;
        private readonly bool _isAutosaveEnabled;

        public string InvoiceConverterConfigsDir => "Invoice Converter";

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

            try
            {
                _settings.UpgradeSettingsIfRequired();
            }
            catch (Exception ex)
            {
                throw new SettingsDataAccessException("Problem with upgrading the configuration data!", ex);
            }

        }

        public void SaveConfig()
        {
            try
            {
                _settings.Save();
            }
            catch (Exception ex)
            {
                throw new SettingsDataAccessException("Problem with saving the settings file!", ex);
            }
        }
    }
}