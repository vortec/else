using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Else.Lib;

namespace Else.Model
{
    /// <summary>
    /// Model for the items displayed as results in the launcher.
    /// </summary>
    public class Result
    {
        public string Title {get; set;}
        public string SubTitle {get; set;}
        public int Index {get;set;}
        public BitmapSource Icon {get; set;}
        // anonymous method that is invoked when the result is selected
        public Action<Query> Launch;
        public bool HasSubTitle {
            get {
                return !SubTitle.IsEmpty();
            }
        }
        public List<Result> ToList() {
            return new List<Result>{
                this
            };
        }
    }
}
