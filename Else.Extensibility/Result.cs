using System;
using System.Collections.Generic;

namespace Else.Extensibility
{
    /// <summary>
    /// A class for wrapping the launch delegate.
    /// <remarks>Because delegates cannot be serialized, we wrap the delegate in a class that provides remoting functionality.</remarks>
    /// </summary>
    public class LaunchDelegateWrapper : MarshalByRefObject
    {
        private readonly Action<Query> _func;

        public LaunchDelegateWrapper(Action<Query> func)
        {
            _func = func;
        }

        public void Invoke(Query query)
        {
            _func?.Invoke(query);
        }
    }

    /// <summary>
    /// Model for the items displayed as results in the launcher.
    /// </summary>
    [Serializable]
    public class Result
    {
        /// <summary>
        /// Supported values:
        /// string: Absolute path to an image file on the filesystem.
        /// string: GetFileIcon://, Direct path to anything on the filesystem (e.g. exe, or folder), from which we extract an image
        /// BitmapSource or Lazy BitmapSource
        /// </summary>
        public object Icon { get; set; }

        /// <summary>
        /// a wrapper around the launch delegate.
        /// </summary>
        public LaunchDelegateWrapper LaunchDelegateWrapper;

        /// <summary>
        /// The anonymous method that will be executed when this result is executed (enter key)
        /// </summary>
        public Action<Query> Launch
        {
            // we wrap the delegate in a class, so that it can be remoted.
            set { LaunchDelegateWrapper = new LaunchDelegateWrapper(value); }
        }

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
        /// Whether this result has a subtitle
        /// </summary>
        public bool HasSubTitle => !string.IsNullOrEmpty(SubTitle);

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