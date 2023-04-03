using Newtonsoft.Json;
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
        public ValidatableBinding()
        {
            // TODO remove dupication
            ValidatesOnNotifyDataErrors = true;
            ValidatesOnExceptions = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        }

        public ValidatableBinding(string path) : base(path)
        {
            ValidatesOnNotifyDataErrors = true;
            ValidatesOnExceptions = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        }
    }
}