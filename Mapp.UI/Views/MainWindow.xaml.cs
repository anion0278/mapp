using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;

namespace Shmap.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //AppCenter.Start("9549dd3a-1371-4a23-b973-f5e80154119d", typeof(Analytics));
            //Analytics.SetEnabledAsync(true);
            //Analytics.TrackEvent("Mapp clicked", new Dictionary<string, string> {
            //    { "Category", "Music" },
            //    { "FileName", "favorite.avi"}
            //});
        }
    }
}
