using System.Windows;

namespace Shmap.Views
{
    /// <summary>
    /// Interaction logic for ManualChange.xaml
    /// </summary>
    public partial class ManualChange : Window
    {
        public ManualChange()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close(); // is it responsibility of view itself to close itself?
        }
    }
}
