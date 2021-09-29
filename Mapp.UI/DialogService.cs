﻿using System.Windows;
using Shmap.CommonServices;
using Shmap.ViewModels;
using Shmap.Views;

namespace Mapp
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
                var result = MessageBox.Show(Application.Current.MainWindow, message, "Upozorneni", MessageBoxButton.YesNo);  
                // TODO how to set owner center for MessageBox? maybe best way is to create own MB?

                if (result == MessageBoxResult.Yes)
                {
                    var window = new ManualChange();
                    window.Owner = Application.Current.MainWindow;  // TODO how to set window owner - main window (to center it)
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