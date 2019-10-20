using System;
using System.Globalization;
using System.Windows.Data;

namespace Martin_App
{
    public class ItemTextToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            InvoiceXML.invoiceInvoiceItem invoiceInvoiceItem = value as InvoiceXML.invoiceInvoiceItem;
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