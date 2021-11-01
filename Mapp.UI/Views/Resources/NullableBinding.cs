using System.Windows.Data;

namespace Shmap.UI.Views.Resources
{
    /// <summary>
    /// This binding is used for numeric textbox, where it is required to return null when text is empty (prop is nullable)
    /// </summary>
    public class NullableBinding : ValidatableBinding
    {
        public NullableBinding() : base()
        {
            TargetNullValue = "";
        }

        public NullableBinding(string path) : base(path)
        {
            TargetNullValue = "";
        }
    }

    public class ValidatableBinding : Binding
    {
        public ValidatableBinding() : base()
        {
            ValidatesOnDataErrors = true;
            ValidatesOnExceptions = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
        }

        public ValidatableBinding(string path) : base(path)
        {
            ValidatesOnDataErrors = true;
            ValidatesOnExceptions = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
        }
    }
}