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
using System.Windows;

namespace wpfmenu
{
    
    public class Engine
    {
        
        
        
        public BindingList<Plugins.QueryResult> resultsCollection = new BindingList<Plugins.QueryResult>();
        public delegate void Handler();
        public event Handler ResultsUpdated;
        
        List<Plugins.Plugin> plugins = new List<Plugins.Plugin>();


        // constructor
        
        public Engine() {
            resultsCollection = new BindingList<Plugins.QueryResult>();
            //resultsCollection.RaiseListChangedEvents = false;
            plugins = new List<Plugins.Plugin>{
                //new Plugins.Programs(),
                new Plugins.Web()
            };
            plugins.ForEach(p => {
                p.Setup();
            });
        }
        public void Launch(Plugins.QueryResult r)
        {
            var result = r.source.Launch(r);
            if (result.close) {
                var parent = Application.Current.MainWindow as LauncherWindow;
                parent.Hide();
            }
        }
        public class QueryInfo {
            public string raw;
            public string[] parts;
            public string first;
            public int length;
            public Plugins.TokenMatch tokenmatch;
            public QueryInfo(string query)
            {
                raw = query;
                parts = query.Split(' ');
                length = query.Length;
                if (length > 0) {
                    first = parts[0];
                }
            }
        }
        
        public void QueryChanged(string query)
        {
            var info = new QueryInfo(query);
            resultsCollection.Clear();

            // not empty
            if (info.length != 0) {
                // check plugins for matches
                foreach (var p in plugins) {
                    info.tokenmatch = p.CheckToken(info);
                    // partial or exact match, ignore partial?
                    if (info.tokenmatch > Plugins.TokenMatch.None) {
                        var results = p.Query(info);
                        foreach (var r in results) {
                            resultsCollection.Add(r);
                        }
                    }
                }
            }
            

            Debug.Print("nresults = {0}", resultsCollection.Count());
            ResultsUpdated.Invoke();
        }
    }
}