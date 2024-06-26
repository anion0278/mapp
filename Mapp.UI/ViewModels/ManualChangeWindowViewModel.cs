﻿using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace Mapp.UI.ViewModels
{
    public interface IManualChangeWindowViewModel
    {
        public bool IsChangeAccepted { get; }
        int MaxLength { get; set; }
        string Message { get; set; }
        string OriginalText { get; set; }
        string EditedText { get; set; }
    }

    public class ManualChangeWindowViewModel : ViewModelBase, IManualChangeWindowViewModel
    {
        // Fody takes care of PropChange notification
        public RelayCommand AcceptChangesCommand { get; } // TODO expose ICommand or IAsyncRelayCommand everywhere

        public bool IsChangeAccepted { get; private set; }

        public string Message { get; set; } // TODO in ViewModel-first approach it can be get-only (set from ctor), same as MaxLength

        public int MaxLength { get; set; } // TODO does not need to be a prop

        public string OriginalText { get; set; } // TODO FIXME does not work!

        public string EditedText { get; set; }

        public int CurrentTextLength => EditedText?.Length ?? 0;

  
        public ManualChangeWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
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