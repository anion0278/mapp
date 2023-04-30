using System.Windows;
using Shmap.Common;
using Shmap.UI.ViewModels;
using Shmap.UI.Views;

namespace Shmap.UI
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        public string AskToChangeLongStringIfNeeded(string message, string textToChange, int maxLength)
        {
            message += $"\nUpravit manualne (Yes), nebo orezat dle maximalni delky {maxLength} (No)?";
            while (textToChange.Length > maxLength) 
            {
                var result = MessageBox.Show(System.Windows.Application.Current.MainWindow, message, "Upozorneni", MessageBoxButton.YesNo);  
                // TODO how to set owner center for MessageBox? maybe best way is to create own MB?

                if (result == MessageBoxResult.Yes)
                {
                    var window = new ManualChange();
                    var vm = window.DataContext as IManualChangeWindowViewModel; // Disadvantage of View-first approach
                    vm.OriginalText = textToChange;
                    vm.EditedText = textToChange;
                    vm.MaxLength = maxLength;
                    vm.Message = message;
                    window.ShowDialog();
                    if (vm.IsChangeAccepted)
                    {
                        textToChange = vm.EditedText;
                    }
                }
                else
                {
                    textToChange = textToChange.Substring(0, maxLength);
                }
            }

            return textToChange;
        }
    }
}