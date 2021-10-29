using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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

    public class DataGris : DataGrid
    {

        protected override void OnExecutedCommitEdit(ExecutedRoutedEventArgs e)
        {
            base.OnExecutedCommitEdit(e);
            if (e.Parameter is DataGridEditingUnit unit && unit == DataGridEditingUnit.Cell)
            {
                CancelEdit(unit);
            }
        }
        //protected override void OnCanExecuteBeginEdit(System.Windows.Input.CanExecuteRoutedEventArgs e)
        //{
        //    var hasCellValidationError = false;
        //    var hasRowValidationError = false;
        //    const BindingFlags bindingFlags =
        //        BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance;
        //    //Current cell
        //    var cellErrorInfo = this.GetType().BaseType.GetProperty("HasCellValidationError", bindingFlags);
        //    //Grid row
        //    var rowErrorInfo = this.GetType().BaseType.GetProperty("HasRowValidationError", bindingFlags);
        //    if (cellErrorInfo != null) hasCellValidationError = (bool)cellErrorInfo.GetValue(this, null);
        //    if (rowErrorInfo != null) hasRowValidationError = (bool)rowErrorInfo.GetValue(this, null);
        //    base.OnCanExecuteBeginEdit(e);
        //    if ((!e.CanExecute && hasCellValidationError) || (!e.CanExecute && hasRowValidationError))
        //    {
        //        e.CanExecute = true;
        //        e.Handled = true;
        //    }
        //}
    }

    public class ValidatableBinding : Binding
    {
        public ValidatableBinding() : base()
        {
            ValidatesOnDataErrors = true;
            ValidatesOnExceptions = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        }

        public ValidatableBinding(string path) : base(path)
        {
            ValidatesOnDataErrors = true;
            ValidatesOnExceptions = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        }
    }
}