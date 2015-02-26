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
        public BindingList<Plugins.Result> resultsCollection = new BindingList<Plugins.Result>();

        
        // loaded plugins
        List<Plugins.Plugin> plugins = new List<Plugins.Plugin>();
        QueryInfo info = new QueryInfo();
        
        public Engine() {
            resultsCollection = new BindingList<Plugins.Result>();
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
        public void Launch(Plugins.Result r)
        {
            r.Launch(info);
            // a result was clicked, or return was pressed, launch the result
            //var result = r.source.Launch(info, r);
            
            //if (result.close) {
            //    var parent = Application.Current.MainWindow as LauncherWindow;
            //    parent.Hide();
            //    return;
            //}
            //if (!result.rewrite_query.IsEmpty()) {
            //    Messenger.Default.Send<NotificationMessage>(new NotificationMessage("rewrite_query", result.rewrite_query), "launcher");
            //}
        }

        // when a query is entered by the user, it is parsed with this class
        public class QueryInfo {
            public string raw;
            public string token;
            public string arguments;
            public bool empty;
            public bool tokenComplete;
            public bool wildcard;
            
            
            public void parse(string query)
            {
                wildcard = false;
                raw = query;
                
                
                int index = query.IndexOf(' ');
                if (index != -1) {
                    // space found, get first word
                    token = query.Substring(0, index);
                    arguments = query.Substring(index+1);
                    tokenComplete = true;
                }
                else {
                    // no spaces
                    token = query;
                    arguments = "";
                    tokenComplete = false;
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
                bool foundPlugin = false;
                var wildcards = new List<Plugins.Plugin>();
                foreach (var p in plugins) {
                    var match = p.CheckToken(info);
                    
                    
                    if (match > Plugins.TokenMatch.None) {
                        if (match == Plugins.TokenMatch.WildCard) {
                            wildcards.Add(p);
                        }
                        else {
                            foundPlugin = true;
                            var results = p.Query(info);
                            foreach (var r in results) {
                                resultsCollection.Add(r);
                            }
                        }
                    }
                }
                if (!foundPlugin) {
                    // no plugin found, show wildcard results
                    info.wildcard = true;
                    foreach (var p in wildcards) {
                        var results = p.Query(info);
                        foreach (var r in results) {
                            resultsCollection.Add(r);
                        }
                    }
                }
            }
            Messenger.Default.Send(new Messages.ResultsUpdated());
        }
    }
}