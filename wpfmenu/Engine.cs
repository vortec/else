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
using GalaSoft.MvvmLight.Messaging;

namespace wpfmenu
{
    
    public class Engine
    {
        // this list is displayed in teh launcher
        public BindingList<Plugins.ResultData> resultsCollection = new BindingList<Plugins.ResultData>();

        // callback for when resultsCollection is updated (todo: move this)
        public delegate void Handler();
        public event Handler ResultsUpdated;
        
        // loaded plugins
        List<Plugins.Plugin> plugins = new List<Plugins.Plugin>();
        QueryInfo info = new QueryInfo();
        
        public Engine() {
            resultsCollection = new BindingList<Plugins.ResultData>();
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
        public void Launch(Plugins.ResultData r)
        {
            // a result was clicked, or return was pressed, launch the result
            var result = r.source.Launch(info, r);
            
            if (result.close) {
                var parent = Application.Current.MainWindow as LauncherWindow;
                parent.Hide();
                return;
            }
            if (!result.rewrite_query.IsEmpty()) {
                Messenger.Default.Send<NotificationMessage>(new NotificationMessage("do something"), "launcher");
            }
        }

        // when a query is entered by the user, it is parsed with this class
        public class QueryInfo {
            public string raw;
            public string token;
            public string arguments;
            public bool empty;
            
            public void parse(string query)
            {
                raw = query;
                
                
                int index = query.IndexOf(' ');
                if (index != -1) {
                    // space found, get first word
                    token = query.Substring(0, index);
                    arguments = query.Substring(index+1);
                }
                else {
                    // no spaces
                    token = query;
                    arguments = "";
                }
                empty = raw.IsEmpty();
                
                raw = query;
            }
        }
        // process the query when it changes
        public void QueryChanged(string query)
        {
            info.parse(query);
            resultsCollection.Clear();

            
            if (!info.empty) {
                // query is not empty, check plugins for results
                foreach (var p in plugins) {
                    var match = p.CheckToken(info);
                    
                    if (match > Plugins.TokenMatch.None) {
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