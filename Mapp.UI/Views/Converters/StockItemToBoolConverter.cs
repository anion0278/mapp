using System;
using System.Globalization;
using System.Windows.Data;

namespace Shmap.UI.Views.Converters
{
    public class StockItemToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return (object)true;
            return (object)false;
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