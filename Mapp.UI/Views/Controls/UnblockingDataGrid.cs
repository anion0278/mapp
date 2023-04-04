using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Shmap.UI.Views.Controls
{
    public class UnblockingDataGrid : DataGrid
    {
        //protected override void OnExecutedCommitEdit(ExecutedRoutedEventArgs e)
        //{
        //    base.OnExecutedCommitEdit(e);
        //    BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance;
        //    PropertyInfo editableItems = this.GetType().BaseType.GetProperty("EditableItems", bindingFlags);
        //    var collectionView = ((System.ComponentModel.IEditableCollectionView)editableItems.GetValue(this));
        //    var item = collectionView.CurrentEditItem;
        //    this.Focus();
        //    collectionView.CommitEdit();
        //}

        protected override void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e)
        {

            bool hasCellValidationError = false;
            bool hasRowValidationError = false;
            BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance;
            //Current cell
            PropertyInfo cellErrorInfo = this.GetType().BaseType.GetProperty("HasCellValidationError", bindingFlags);
            //Grid level
            PropertyInfo rowErrorInfo = this.GetType().BaseType.GetProperty("HasRowValidationError", bindingFlags);

            if (cellErrorInfo != null) hasCellValidationError = (bool)cellErrorInfo.GetValue(this, null);
            if (rowErrorInfo != null) hasRowValidationError = (bool)rowErrorInfo.GetValue(this, null);

            base.OnCanExecuteBeginEdit(e);
            if (!e.CanExecute && !hasCellValidationError && hasRowValidationError)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }
    }

    public class HyperlinkCommandColumn : System.Windows.Controls.DataGridHyperlinkColumn
    {
        /// <summary>
        /// Support binding the hyperlink to an ICommand rather than a Uri
        /// </summary>
        public BindingBase Command { get; set; }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var result = base.GenerateElement(cell, dataItem);

            if (((TextBlock)result).Inlines.FirstInline is Hyperlink link)
                BindingOperations.SetBinding(link, Hyperlink.CommandProperty, Command);

            return result;
        }
    }

}