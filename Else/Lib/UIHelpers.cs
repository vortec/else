using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Else.Lib
{
    public static class UIHelpers
    {
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }
        public static Window GetWindow<T>(string name="") where T : Window
        {
            var window = Application.Current.Windows.OfType<T>().FirstOrDefault(w => w.Name.Equals(name));
            return window;
        }
        public static bool FocusWindow<T>(string name="") where T : Window
        {
            var window = GetWindow<T>(name);
            if (window != null) {
                window.Focus();
                return true;
            }
            return false;
        }

        public static BitmapImage LoadImageFromResources(string path)
        {
            var uri = new Uri("pack://application:,,,/Else;component/Resources/" + path);
            return new BitmapImage(uri);
        }
    }
}
