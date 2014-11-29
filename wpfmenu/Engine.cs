using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace wpfmenu
{
    
    
    
    class Engine
    {
        public class Result
        {
            public string Title {get; set;}
            public BitmapSource Icon {get; set;}
        }
        public class ResultsCollection : ObservableCollection<Result>
        {
            public ResultsCollection() : base()
            {
            }
        }
        
        // members
        
        public ResultsCollection results { get; set; }
        

        // constructor
        Plugin_Programs p;
        public Engine() {
            results = new ResultsCollection();
            
            p = new Plugin_Programs();
            p.Setup();
        }
        public void QueryChanged(string query)
        {
            results.Clear();
            query = query.Trim();

            if (query.Length == 0) {
                // empty
            }
            else {
                var xresults = p.Query(query);
                
                foreach (var r in xresults) {
                    results.Add(r);
                }
                
                var nresults = xresults.Count;
                Debug.Print("nresults = {0}", nresults);
            }
        }
    }
}
