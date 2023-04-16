using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Shmap.UI.Views.Converters
{
    public class ProductsListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (object)string.Join(Environment.NewLine, (value as IEnumerable<string>));
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
            return string.Join(Environment.NewLine, (value as IEnumerable<string>));
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