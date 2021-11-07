using System;
using System.ComponentModel;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Shmap.ViewModels
{
    public interface IManualChangeWindowViewModel
    {
        public bool IsChangeAccepted { get; }
        int MaxLength { get; set; }
        string Message { get; set; }
        string OriginalText { get; set; }
        string EditedText { get; set; }
        int CurrentTextLength { get; }
    }

    public class ManualChangeWindowViewModel : ViewModelWithErrorValidationBase, IManualChangeWindowViewModel
    {
        // Fody takes care of PropChange notification
        public RelayCommand AcceptChangesCommand { get; }

        public bool IsChangeAccepted { get; private set; }

        public string Message { get; set; } // TODO in ViewModel-first approach it can be get-only (set from ctor), same as MaxLength

        public int MaxLength { get; set; }

        public string OriginalText { get; set; }

        public string EditedText { get; set; }

        public int CurrentTextLength => EditedText?.Length ?? 0;

        public ManualChangeWindowViewModel() // design time ctor
        {
            if (IsInDesignMode)
            {
                OriginalText = "Original text example";
                MaxLength = 30;
                EditedText = "Edited text example";
                Message = "Nazev mesta/zeme je prilis dlouhy v objednavce C.: 751-548-846G" + "\n multiline\n multiline\n multiline";
            }

            AcceptChangesCommand = new RelayCommand(AcceptChanges, AcceptedChangesCanExecute);
            IsChangeAccepted = false;
        }

        private bool AcceptedChangesCanExecute()
        {
            return CurrentTextLength <= MaxLength;
        }

        private void AcceptChanges()
        {
            IsChangeAccepted = true;
        }
    }
}