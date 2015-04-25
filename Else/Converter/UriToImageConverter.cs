using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Else.Converter
{
    public class UriToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // no image provided
            if (value == null) {
                return null;
            }
            // try to convert
            try {
                if (value is string) {
                    return Helpers.UI.LoadImageFromPath((string) value).Value;
                }
                if (value is Lazy<BitmapSource> || value is BitmapSource) {
                    return value;
                }
                return null;
            }
            catch {
                return new BitmapImage();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
