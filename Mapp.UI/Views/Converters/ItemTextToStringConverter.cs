using System;
using System.Globalization;
using System.Windows.Data;

namespace Shmap.UI.Views.Converters
{
    public class ItemTextToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            InvoiceXml.invoiceInvoiceItem invoiceInvoiceItem = value as InvoiceXml.invoiceInvoiceItem;
            if (invoiceInvoiceItem.stockItem == null)
                return (object)invoiceInvoiceItem.text;
            return (object)string.Empty;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}