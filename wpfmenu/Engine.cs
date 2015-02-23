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
        // this list is displayed in teh launcher
        public BindingList<Plugins.QueryResult> resultsCollection = new BindingList<Plugins.QueryResult>();

        // callback for when resultsCollection is updated (todo: move this)
        public delegate void Handler();
        public event Handler ResultsUpdated;
        
        // loaded plugins
        List<Plugins.Plugin> plugins = new List<Plugins.Plugin>();
        
        public Engine() {
            resultsCollection = new BindingList<Plugins.QueryResult>();
            //resultsCollection.RaiseListChangedEvents = false;

            // load plugins
            plugins = new List<Plugins.Plugin>{
                //new Plugins.Programs(),
                new Plugins.Web()
            };
            // setup plugins
            plugins.ForEach(p => {
                p.Setup();
            });
        }
        public void Launch(Plugins.QueryResult r)
        {
            // a result was clicked, or return was pressed, launch the result
            var result = r.source.Launch(r);
            if (result.close) {
                var parent = Application.Current.MainWindow as LauncherWindow;
                parent.Hide();
            }
        }

        // when a query is entered by the user, it is parsed with this class
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
        // process the query when it changes
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
            
            ResultsUpdated.Invoke();
        }
    }
}