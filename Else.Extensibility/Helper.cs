using System;
using System.Windows.Media.Imaging;

namespace Else.Extensibility
{
    public class Helper
    {
        /// <summary>
        /// Returns a lazy BitmapSource for a given path within the Resources directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Lazy<BitmapSource> LoadImageFromResources(string path)
        {
            return new Lazy<BitmapSource>(() => {
                var uri = new Uri("pack://application:,,,/Else;component/Resources/" + path);
                return new BitmapImage(uri);
            });
        }
    }
}
