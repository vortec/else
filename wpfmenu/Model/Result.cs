using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace wpfmenu.Model
{
    public class Result
    {
        public string Title {get; set;}
        public BitmapSource Icon {get; set;}
        public string SubTitle {get; set;}
        // overridden by subclass
        public Action<Model.QueryInfo, Result> Launch;
        public object data;
        public int Index {get;set;}
        public bool IsSingular {
            get {
                return SubTitle.IsEmpty();
            }
            
        }
    }
}
