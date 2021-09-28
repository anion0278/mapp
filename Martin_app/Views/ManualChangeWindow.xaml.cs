using System.Windows;

namespace Shmap.Views
{
    /// <summary>
    /// Interaction logic for ManualChange.xaml
    /// </summary>
    public partial class ManualChange : Window
    {
        public int MaxLength { get; set; }

        public ManualChange(int maxLength, string initialText)
        {
            MaxLength = maxLength;
            InitialText.Text = initialText;
            ChangedText.Text = initialText;

            InitializeComponent();
        }

        public string CorrectedText => ChangedText.Text;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
