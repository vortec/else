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
        public string SubTitle {get; set;}
        public BitmapSource Icon {get; set;}
        public Action Launch;
        public int Index {get;set;}
        public bool IsSingular {
            get {
                return SubTitle.IsEmpty();
            }
        }
    }
}
