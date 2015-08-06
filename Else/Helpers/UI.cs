using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Else.Extensibility;

namespace Else.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class UI
    {
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static Window GetWindow<T>(string name = "") where T : Window
        {
            var window = Application.Current.Windows.OfType<T>().FirstOrDefault(w => w.Name.Equals(name));
            return window;
        }

        public static bool FocusWindow<T>(string name = "") where T : Window
        {
            var window = GetWindow<T>(name);
            if (window != null) {
                window.Focus();
                return true;
            }
            return false;
        }

        public static void UiInvoke(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        /// <remarks><see cref="http://stackoverflow.com/a/1759923"/></remarks>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null) {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName)) {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName) {
                        // if the child's name is of the request name
                        foundChild = (T) child;
                        break;
                    }
                }
                else {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj, string childName = null) where T : DependencyObject
        {
            if (depObj != null) {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    var name = (string) child.GetValue(FrameworkElement.NameProperty);

                    if (child is T) {
                        if (string.IsNullOrEmpty(childName) || name == childName) {
                            yield return (T) child;
                        }
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child, childName)) {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static UIElement FindChildByTypeName(DependencyObject parent, string typeName)
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            UIElement foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);

                // If the child is not of the request child type child
                var childElement = child as UIElement;
                if (childElement != null && childElement.GetType().Name == typeName) {
                    foundChild = childElement;
                }
                else {
                    // recurse
                    foundChild = FindChildByTypeName(child, typeName);
                    if (foundChild != null) {
                        break;
                    }
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Provides an image from a given path.
        /// 
        /// Supported values:
        /// string: Absolute path to an image file on the filesystem.
        /// string: GetFileIcon://, Direct path to anything on the filesystem (e.g. exe, or folder), from which we extract an image
        /// BitmapSource or Lazy BitmapSource
        /// 
        /// </summary>
        /// <returns></returns>
        public static Lazy<BitmapSource> LoadImageFromPath(string uri)
        {
            const string iconScheme = "GetFileIcon://";
            const string appResourceSchema = "AppResource://";

            return new Lazy<BitmapSource>(() =>
            {
                if (string.IsNullOrEmpty(uri)) {
                    // return blank image
                    var image = BitmapSource.Create(2, 2, 32, 32, PixelFormats.Indexed1, new BitmapPalette(new List<Color> {Colors.Transparent}),
                        new byte[] {0, 0, 0, 0}, 1);
                    return image;
                }
                // if uri starts with "GetFileIcon://", instead use IconTools to query the operating system for an icon
                // if the path is an executable (.exe), the exe icon is returned
                // for any other path, the image should be similar to what explorer.exe would show (this should work for directories too)
                if (uri.StartsWith(iconScheme)) {
                    var path = uri.Substring(iconScheme.Length);
                    return IconTools.GetBitmapForFile(path).Value;
                }
                if (uri.StartsWith(appResourceSchema)) {
                    var path = uri.Substring(appResourceSchema.Length);
                    uri = "pack://application:,,,/Else;component/Resources/" + path;
                }

                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
                bi.CreateOptions = BitmapCreateOptions.DelayCreation;
                bi.CacheOption = BitmapCacheOption.Default;
                bi.EndInit();
                return bi;
            });
        }
    }
}