using Shmap.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Shmap.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
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
