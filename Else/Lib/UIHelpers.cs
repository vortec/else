using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
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
                        foundChild = (T)child;
                        break;
                    }
                }
                else {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj, string childName=null) where T : DependencyObject
        {
            if (depObj != null) {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    var name = (string)child.GetValue(FrameworkElement.NameProperty);

                    if (child is T) {
                        if (string.IsNullOrEmpty(childName) || name == childName) {
                            yield return (T)child;
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

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);

                // If the child is not of the request child type child
                UIElement childElement = child as UIElement;
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

        

        public static Lazy<BitmapSource> LoadImageFromResources(string path)
        {
            return new Lazy<BitmapSource>(() => {
                var uri = new Uri("pack://application:,,,/Else;component/Resources/" + path);
                return new BitmapImage(uri);
            });
        }
    }
}
