using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace Shmap.UI.Views.Resources
{
    public class ValidatableDataGrid : DataGrid
    {
        //protected override void OnExecutedCommitEdit(ExecutedRoutedEventArgs e)
        //{
        //    base.OnExecutedCommitEdit(e);
        //    BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance;
        //    PropertyInfo editableItems = this.GetType().BaseType.GetProperty("EditableItems", bindingFlags);
        //    ((System.ComponentModel.IEditableCollectionView)editableItems.GetValue(this)).CommitEdit();
        //}


        //protected override void OnCanExecuteBeginEdit(System.Windows.Input.CanExecuteRoutedEventArgs e)
        //{

        //    bool hasCellValidationError = false;
        //    bool hasRowValidationError = false;
        //    BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance;
        //    //Current cell
        //    PropertyInfo cellErrorInfo = this.GetType().BaseType.GetProperty("HasCellValidationError", bindingFlags);
        //    //Grid level
        //    PropertyInfo rowErrorInfo = this.GetType().BaseType.GetProperty("HasRowValidationError", bindingFlags);

        //    if (cellErrorInfo != null) hasCellValidationError = (bool)cellErrorInfo.GetValue(this, null);
        //    if (rowErrorInfo != null) hasRowValidationError = (bool)rowErrorInfo.GetValue(this, null);

        //    base.OnCanExecuteBeginEdit(e);
        //    if (!e.CanExecute && !hasCellValidationError && hasRowValidationError)
        //    {
        //        e.CanExecute = true;
        //        e.Handled = true;
        //    }
        //}
    }
}