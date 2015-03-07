using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;

namespace wpfmenu.Model
{
    
    public class Result
    {
        public string Title {get; set;}
        public string SubTitle {get; set;}
        public int Index {get;set;}
        public BitmapSource Icon {get; set;}
        // anonymous method that is invoked when the result is selected
        public Action Launch;
        public bool HasSubTitle {
            get {
                return !SubTitle.IsEmpty();
            }
        }
    }
}
