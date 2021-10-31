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
        int CurrentTextLength { get; set; }
    }

    public class ManualChangeWindowViewModel: ViewModelWithErrorValidationBase, IManualChangeWindowViewModel
    {
        private int _maxLength;
        private string _originalText;
        private string _editedText;
        private int _currentTextLength;
        private string _message;

        public RelayCommand AcceptChangesCommand { get; }

        public bool IsChangeAccepted { get; private set; }

        public string Message // TODO in ViewModel-first approach it can be get-only (set from ctor), same as MaxLength
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public int MaxLength
        {
            get => _maxLength;
            set => Set(ref _maxLength, value);
        }

        public string OriginalText
        {
            get => _originalText;
            set => Set(ref _originalText, value);
        }

        public string EditedText
        {
            get => _editedText;
            set
            {
                Set(ref _editedText, value);
                CurrentTextLength = EditedText.Length;
            }
        }

        public int CurrentTextLength
        {
            get => _currentTextLength;
            set => Set(ref _currentTextLength, value);
        }

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

        private void AcceptChanges() //TODO possbile to apply validataion rules
        {
            IsChangeAccepted = true;
        }
    }
}