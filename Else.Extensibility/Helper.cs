using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Else.Extensibility
{
    public class Helper
    {
        /// <summary>
        /// Returns a lazy BitmapSource for a given path within the Resources directory.
        /// Checks only the calling assembly.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Lazy<BitmapSource> LoadImageFromResources(string path)
        {
            // store the calling assembly
            var callingAssemblyName = Assembly.GetCallingAssembly().GetName().Name;

            // this method will be called by wpf, so we make it use the calling assembly above
            return new Lazy<BitmapSource>(() =>
            {
                var fullPath = Path.Combine("/Resources", path);
                var uriString = string.Format("pack://application:,,,/{0};component{1}", callingAssemblyName, fullPath);
                return new BitmapImage(new Uri(uriString));
            });
        }
    }
}