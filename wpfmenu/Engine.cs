using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.ComponentModel;

namespace wpfmenu
{
    public class Engine
    {
        public class Result
        {
            public string Title {get; set;}
            public BitmapSource Icon {get; set;}
            public string SubTitle {get; set;}
        }
        
        public BindingList<Result> results = new BindingList<Result>();
        public delegate void Handler();
        public event Handler ResultsUpdated;
        
        List<Plugin> plugins = new List<Plugin>();


        // constructor
        
        public Engine() {
            results = new BindingList<Result>();
            results.RaiseListChangedEvents = false;
            var p = new Plugin_Programs();
            p.Setup();
            plugins.Add(p);

            
        }
        public void Launch(Result r)
        {
            Debug.Print("launching {0}", r);
        }
        public void QueryChanged(string query)
        {
            results.Clear();
            query = query.Trim();

            if (query.Length == 0) {
                // empty
            }
            else {
                var xa = plugins.Select(obj => obj.Query(query));
                foreach (var x in xa) {
                    foreach (var y in x) {
                        results.Add(y);
                    }
                }
                ResultsUpdated.Invoke();
            }
        }
    }
}

