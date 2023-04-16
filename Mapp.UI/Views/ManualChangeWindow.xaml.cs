using System.Windows;

namespace Shmap.UI.Views
{
    /// <summary>
    /// Interaction logic for ManualChange.xaml
    /// </summary>
    public partial class ManualChange : Window
    {
        public ManualChange()
        {
            InitializeComponent();

            // TODO helper method to get current active window of the app
            Owner = Application.Current.MainWindow;  // its responsibility of window itself to set its parent
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close(); // is it responsibility of view itself to close itself?
        }
    }
}
