using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Shmap.UI.Views.Controls
{

    public class DataGridNumberColumn : DataGridTextColumn
    {
        private TextBoxInputBehavior _behavior;

        public static readonly DependencyProperty MaxInputLengthProperty = DependencyProperty.Register(
            nameof(MaxInputLength),
            typeof(int),
            typeof(TextBoxInputBehavior),
            new FrameworkPropertyMetadata(int.MinValue));

        public int MaxInputLength
        {
            get { return (int)GetValue(MaxInputLengthProperty); }
            set { SetValue(MaxInputLengthProperty, value); }
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var element = base.GenerateElement(cell, dataItem);

            // A clever workaround the StringFormat issue with the Binding set to the 'Binding' property. If you use StringFormat it
            // will only work in edit mode if you changed the value, otherwise it will retain formatting when you enter editing.
            if (!string.IsNullOrEmpty(StringFormat))
            {
                BindingOperations.ClearBinding(element, TextBlock.TextProperty);
                BindingOperations.SetBinding(element, FrameworkElement.TagProperty, Binding);
                BindingOperations.SetBinding(element,
                    TextBlock.TextProperty,
                    new Binding
                    {
                        Source = element,
                        Path = new PropertyPath("Tag"),
                        StringFormat = StringFormat
                    });
            }

            return element;
        }

        public class TextBoxInputBehavior : Behavior<TextBox>
        {
            #region DependencyProperties

            public static readonly DependencyProperty RegularExpressionProperty = DependencyProperty.Register(
                nameof(RegularExpression),
                typeof(string),
                typeof(TextBoxInputBehavior),
                new FrameworkPropertyMetadata(".*"));

            public string RegularExpression
            {
                get
                {
                    if (IsInteger) // TODO add minus property
                        return @"^[0-9]+$";
                    if (IsNumeric)
                        return @"^[0-9.]+$";
                    return (string)GetValue(RegularExpressionProperty);
                }
                set { SetValue(RegularExpressionProperty, value); }
            }

            public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
                nameof(MaxLength),
                typeof(int),
                typeof(TextBoxInputBehavior),
                new FrameworkPropertyMetadata(int.MinValue));

            public int MaxLength
            {
                get { return (int)GetValue(MaxLengthProperty); }
                set { SetValue(MaxLengthProperty, value); }
            }

            public static readonly DependencyProperty EmptyValueProperty = DependencyProperty.Register(
                nameof(EmptyValue),
                typeof(string),
                typeof(TextBoxInputBehavior));

            public string EmptyValue
            {
                get { return (string)GetValue(EmptyValueProperty); }
                set { SetValue(EmptyValueProperty, value); }
            }

            public static readonly DependencyProperty IsNumericProperty = DependencyProperty.Register(
                nameof(IsNumeric),
                typeof(bool),
                typeof(TextBoxInputBehavior));

            public bool IsNumeric
            {
                get { return (bool)GetValue(IsNumericProperty); }
                set { SetValue(IsNumericProperty, value); }
            }

            public static readonly DependencyProperty IsIntegerProperty = DependencyProperty.Register(
                nameof(IsInteger),
                typeof(bool),
                typeof(TextBoxInputBehavior));

            public bool IsInteger
            {
                get { return (bool)GetValue(IsIntegerProperty); }
                set
                {
                    if (value)
                        SetValue(IsNumericProperty, true);
                    SetValue(IsIntegerProperty, value);
                }
            }

            public static readonly DependencyProperty AllowSpaceProperty = DependencyProperty.Register(
                nameof(AllowSpace),
                typeof(bool),
                typeof(TextBoxInputBehavior));

            public bool AllowSpace
            {
                get { return (bool)GetValue(AllowSpaceProperty); }
                set { SetValue(AllowSpaceProperty, value); }
            }

            #endregion

            protected override void OnAttached()
            {
                base.OnAttached();

                AssociatedObject.PreviewTextInput += PreviewTextInputHandler;
                AssociatedObject.PreviewKeyDown += PreviewKeyDownHandler;
                DataObject.AddPastingHandler(AssociatedObject, PastingHandler);
            }

            protected override void OnDetaching()
            {
                base.OnDetaching();

                if (AssociatedObject == null)
                    return;

                AssociatedObject.PreviewTextInput -= PreviewTextInputHandler;
                AssociatedObject.PreviewKeyDown -= PreviewKeyDownHandler;
                DataObject.RemovePastingHandler(AssociatedObject, PastingHandler);
            }

            private void PreviewTextInputHandler(object sender, TextCompositionEventArgs e)
            {
                string text;
                if (AssociatedObject.Text.Length < AssociatedObject.CaretIndex)
                    text = AssociatedObject.Text;
                else
                    text = TreatSelectedText(out var remainingTextAfterRemoveSelection)
                        ? remainingTextAfterRemoveSelection.Insert(AssociatedObject.SelectionStart, e.Text)
                        : AssociatedObject.Text.Insert(AssociatedObject.CaretIndex, e.Text);
                e.Handled = !ValidateText(text);
            }

            private void PreviewKeyDownHandler(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Space)
                    e.Handled = !AllowSpace;

                if (string.IsNullOrEmpty(EmptyValue))
                    return;

                string text = null;

                // Handle the Backspace key
                if (e.Key == Key.Back)
                {
                    if (!TreatSelectedText(out text))
                    {
                        if (AssociatedObject.SelectionStart > 0)
                            text = AssociatedObject.Text.Remove(AssociatedObject.SelectionStart - 1, 1);
                    }
                }
                // Handle the Delete key
                else if (e.Key == Key.Delete)
                {
                    // If text was selected, delete it
                    if (!TreatSelectedText(out text) && AssociatedObject.Text.Length > AssociatedObject.SelectionStart)
                    {
                        // Otherwise delete next symbol
                        text = AssociatedObject.Text.Remove(AssociatedObject.SelectionStart, 1);
                    }
                }

                if (text == string.Empty)
                {
                    AssociatedObject.Text = EmptyValue;
                    if (e.Key == Key.Back)
                        AssociatedObject.SelectionStart++;
                    e.Handled = true;
                }
            }

            private void PastingHandler(object sender, DataObjectPastingEventArgs e)
            {
                if (e.DataObject.GetDataPresent(DataFormats.Text))
                {
                    var text = Convert.ToString(e.DataObject.GetData(DataFormats.Text));

                    if (!ValidateText(text))
                        e.CancelCommand();
                }
                else
                    e.CancelCommand();
            }

            public bool ValidateText(string text)
            {
                return new Regex(RegularExpression, RegexOptions.IgnoreCase).IsMatch(text) && (MaxLength == int.MinValue || text.Length <= MaxLength);
            }

            /// <summary>
            /// Handle text selection.
            /// </summary>
            /// <returns>true if the character was successfully removed; otherwise, false.</returns>
            private bool TreatSelectedText(out string text)
            {
                text = null;
                if (AssociatedObject.SelectionLength <= 0)
                    return false;

                var length = AssociatedObject.Text.Length;
                if (AssociatedObject.SelectionStart >= length)
                    return true;

                if (AssociatedObject.SelectionStart + AssociatedObject.SelectionLength >= length)
                    AssociatedObject.SelectionLength = length - AssociatedObject.SelectionStart;

                text = AssociatedObject.Text.Remove(AssociatedObject.SelectionStart, AssociatedObject.SelectionLength);
                return true;
            }
        }


        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            if (!(editingElement is TextBox textBox))
                return null;

            var originalText = textBox.Text;

            _behavior = new TextBoxInputBehavior
            {
                IsNumeric = true,
                EmptyValue = EmptyValue,
                IsInteger = IsInteger,
                MaxLength = MaxInputLength
            };

            _behavior.Attach(textBox);

            textBox.Focus();

            if (editingEventArgs is TextCompositionEventArgs compositionArgs) // User has activated editing by already typing something
            {
                if (compositionArgs.Text == "\b") // Backspace, it should 'clear' the cell
                {
                    textBox.Text = EmptyValue;
                    textBox.SelectAll();
                    return originalText;
                }

                if (_behavior.ValidateText(compositionArgs.Text))
                {
                    textBox.Text = compositionArgs.Text;
                    textBox.Select(textBox.Text.Length, 0);
                    return originalText;
                }
            }

            if (!(editingEventArgs is MouseButtonEventArgs) || !PlaceCaretOnTextBox(textBox, Mouse.GetPosition(textBox)))
                textBox.SelectAll();

            return originalText;
        }

        private static bool PlaceCaretOnTextBox(TextBox textBox, Point position)
        {
            int characterIndexFromPoint = textBox.GetCharacterIndexFromPoint(position, false);
            if (characterIndexFromPoint < 0)
                return false;
            textBox.Select(characterIndexFromPoint, 0);
            return true;
        }

        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            UnwireTextBox();
            base.CancelCellEdit(editingElement, uneditedValue);
        }

        protected override bool CommitCellEdit(FrameworkElement editingElement)
        {
            UnwireTextBox();
            return base.CommitCellEdit(editingElement);
        }

        private void UnwireTextBox() => _behavior.Detach();

        public static readonly DependencyProperty EmptyValueProperty = DependencyProperty.Register(
            nameof(EmptyValue),
            typeof(string),
            typeof(DataGridNumberColumn));

        public string EmptyValue
        {
            get => (string)GetValue(EmptyValueProperty);
            set => SetValue(EmptyValueProperty, value);
        }

        public static readonly DependencyProperty IsIntegerProperty = DependencyProperty.Register(
            nameof(IsInteger),
            typeof(bool),
            typeof(DataGridNumberColumn));

        public bool IsInteger
        {
            get => (bool)GetValue(IsIntegerProperty);
            set => SetValue(IsIntegerProperty, value);
        }

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
            nameof(StringFormat),
            typeof(string),
            typeof(DataGridNumberColumn));

        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }
    }

    public class DataGridNumericColumn : DataGridTextColumn
    {
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            var edit = editingElement as TextBox;
            edit.PreviewTextInput += Edit_PreviewTextInput;
            DataObject.AddPastingHandler(edit, OnPaste);
            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var data = e.SourceDataObject.GetData(DataFormats.Text);
            if (!IsDataValid(data)) e.CancelCommand();
        }

        private void Edit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDataValid(e.Text);
        }

        bool IsDataValid(object data)
        {
            try
            {
                Convert.ToInt32(data);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }


    public abstract class RestrictedInputTextBoxBase: TextBox
    {
        protected virtual Regex AllowedRegexPattern { get; set; }

        protected RestrictedInputTextBoxBase()
        {
            DataObject.AddPastingHandler(this, PastingHandler);
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsSpaceAllowed() && e.Key == Key.Space) //handling space character
                e.Handled = true;
        }

        private bool IsSpaceAllowed()
        {
            return IsTextAllowed(" ");
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text))
                e.Handled = true;
            base.OnPreviewTextInput(e);
        }

        private bool IsTextAllowed(string text)
        {
            return AllowedRegexPattern.IsMatch(text);
        }

        protected virtual void PastingHandler(object sender, DataObjectPastingEventArgs e) // handles also drag-drop-paste
        {
            
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsSpaceAllowed() && !string.IsNullOrWhiteSpace(text)) // handles unwanted space characters when copy-pasting
                {
                    text = text.Trim();
                }
                if (!IsTextAllowed(text)) e.CancelCommand();
            }
            else e.CancelCommand();
        }
    }

    public class NumericTextBox :  RestrictedInputTextBoxBase
    {
        protected override Regex AllowedRegexPattern { get; set; } = new ("^[0-9]+$");
    }

    public class EmailTextBox : RestrictedInputTextBoxBase
    {
        protected override Regex AllowedRegexPattern { get; set; } = new (@"^[0-9A-Za-z@_.]+$"); 
    }

  
}