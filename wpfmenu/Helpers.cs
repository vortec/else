using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace wpfmenu
{
    /// <summary>
    /// Helper methods for UI
    /// </summary>
    public static class Helpers
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
                Debug.Print("focusing window");
                window.Focus();
                return true;
            }
            return false;
        }
    }
}
