using System.Windows;
using Shmap.CommonServices;

namespace Mapp
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        public string AskToChangeLongStringIfNeeded(string message, string str, int maxLength)
        {
            message += $". Upravit manualne (Yes), nebo orezat dle maximalni delky {maxLength} (No)";
            while (str.Length > maxLength)
            {
                var result = MessageBox.Show(message, "Upozorneni", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var window = new ManualChange(maxLength, str);
                    window.ShowDialog();
                    str = window.CorrectedText;
                }
                else
                {
                    str = str.Substring(0, maxLength);
                }
            }

            return str;
        }
    }
}