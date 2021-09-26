using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mapp
{
    public abstract class RestrictedInputTextBoxBase: TextBox
    {
        protected virtual Regex AllowedRegexPattern { get; set; } 

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!AllowedRegexPattern.IsMatch(e.Text))
                e.Handled = true;
            base.OnPreviewTextInput(e);
        }

    }

    public class NumericTextBox :  RestrictedInputTextBoxBase
    {
        protected override Regex AllowedRegexPattern { get; set; } = new Regex("^[0-9]+$");
    }
}