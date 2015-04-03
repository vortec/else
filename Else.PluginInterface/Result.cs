using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Else.PluginInterface
{
    /// <summary>
    /// Model for the items displayed as results in the launcher.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Main text
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Sub text 
        /// </summary>
        public string SubTitle { get; set; }
        /// <summary>
        /// internal
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Image displayed in the launcher (lazy loaded)
        /// </summary>
        public Lazy<BitmapSource> Icon { get; set; }

        /// <summary>
        /// Whether this result has a subtitle
        /// </summary>
        public bool HasSubTitle => !string.IsNullOrEmpty(SubTitle);
        
        /// <summary>
        /// anonymous method that is invoked when the result is selected
        /// </summary>
        public Action<Query> Launch;

        /// <summary>
        /// Convert this single result object to a List of Results
        /// </summary>
        /// <returns></returns>
        public List<Result> ToList()
        {
            return new List<Result>
            {
                this
            };
        }
    }
}