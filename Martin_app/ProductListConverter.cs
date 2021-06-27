using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Mapp
{
    public class ProductsListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (object)string.Join(Environment.NewLine, (value as IEnumerable<InvoiceXML.invoiceInvoiceItem>).Select(p => p.text));
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
    public class ProductsListToSkuConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (object)string.Join(Environment.NewLine, (value as IEnumerable<InvoiceXML.invoiceInvoiceItem>).Select(p => p.amazonSkuCode));
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