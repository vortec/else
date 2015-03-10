using System;
using System.Windows.Data;

namespace Else.Converter
{
    public class EqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is int && values[1] is int) {
                return (int)values[0] == (int)values[1];
            }
            return false;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
