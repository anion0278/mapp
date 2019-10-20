using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Martin_app
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
            InitializeComponent();
            InitialText.Text = initialText;
            ChangedText.Text = initialText;
        }

        public string CorrectedText => ChangedText.Text;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
