//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Diagnostics;
//using System.Runtime.Caching;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using Else.Extensibility;
//
//namespace Else.Lib
//{
//    internal static class ImageCache
//    {
//        private static readonly MemoryCache _cache;
//
//        static ImageCache()
//        {
//            var config = new NameValueCollection
//            {
//                {"CacheMemoryLimitMegabytes", "1"}
//            };
//            _cache = new MemoryCache("Images", config);
//        }
//
//        public static Lazy<BitmapSource> GetImage(string uri)
//        {
//            const string iconScheme = "GetFileIcon://";
//
//
//            return new Lazy<BitmapSource>(() =>
//            {
//                
//                // first try and return from the cache
//                
//                if (!string.IsNullOrEmpty(uri)) {
//                    var cached = _cache.Get(uri);
//                    if (cached != null) {
//                        Debug.Print("loaded image from cache: {0}", uri);
//                        return cached as BitmapSource;
//                    }
//                }
//
//                BitmapSource loaded = null;
//
//                // if uri is empty, return an empty file.
//                if (!string.IsNullOrEmpty(uri)) {
//                    // special behaviour for GetFileIcon:// scheme
//                    if (uri.StartsWith(iconScheme)) {
//                        var iconpath = uri.Substring(iconScheme.Length);
//                        loaded = IconTools.GetBitmapForFile(iconpath).Value;
//                        Debug.Print("loaded image as icon: {0}", uri);
//                    }
//                    // standard uri schema
//                    else {
//                        var bi = new BitmapImage();
//                        bi.BeginInit();
//                        bi.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
//                        bi.CreateOptions = BitmapCreateOptions.DelayCreation;
//                        bi.CacheOption = BitmapCacheOption.Default;
//                        bi.EndInit();
//                        loaded = bi;
//                        Debug.Print("loaded image - standard: {0}", uri);
//                    }
//                }
//
//                if (loaded != null) {
//                    // we found the image, add to the cache
//                    var cip = new CacheItemPolicy
//                    {
//                        SlidingExpiration = new TimeSpan(0, 0, 5)
//                    };
//                    Debug.Print("set cache: {0}", uri);
//                    _cache.Set(uri, loaded, cip);
//                    return loaded;
//                }
//                // unable to load the image
//                Debug.Print("Failed to load image: {0}", uri);
//                // return blank image
//                var image = BitmapSource.Create(2, 2, 32, 32, PixelFormats.Indexed1, new BitmapPalette(new List<Color> {Colors.Transparent}),
//                    new byte[] {0, 0, 0, 0}, 1);
//                return image;
//            });
//        }
//    }
//}