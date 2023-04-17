using Shmap.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Shmap.CommonServices;
using Shmap.UI.Settings;

namespace Shmap.UI.Views
{

    public class WindowWithSettings : Window
    {
        public ISettingsWrapper SettingsWrapper { get; protected set; }
    }

    public partial class MainWindow : WindowWithSettings
    {
        public MainWindow(IMainViewModel viewModel, ISettingsWrapper settingsWrapper)
        {
            SettingsWrapper = settingsWrapper;
            InitializeComponent();
            DataContext = viewModel;
            Title = FormatTitleAssemblyFileVersion(Assembly.GetEntryAssembly());
        }

        private string FormatTitleAssemblyFileVersion(Assembly assembly)
        {
            try
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
                return "Mapp v" + new Version(fileVersion.FileVersion).ToString(3);
            }
            catch (System.Exception)
            {
                return "Mapp v" + (assembly?.GetName()?.Version?.ToString(3) ?? "Version unavailable");
            }
        }
    }
}
