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
                new Plugins.Web(),
                new Plugins.Programs()
            };
            // setup plugins
            plugins.ForEach(p => {
                p.Setup();
            });
        }
        public void Launch(Plugins.Result r)
        {
            r.Launch(info, r);
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
                bool showDefaultResults = true;
                var defaults = new List<Plugins.Plugin>();
                foreach (var p in plugins) {
                    var match = p.CheckToken(info);
                    if (match == Plugins.TokenMatch.Generic) {
                        defaults.Add(p);
                    }
                    else {
                        var results = p.Query(info);
                        if (results.Count() > 0) {
                            foreach (var r in results) {
                                resultsCollection.Add(r);
                            }
                            showDefaultResults = false;
                        }
                    }
                }
                if (showDefaultResults) {
                    
                    // no plugin found, show wildcard results
                    info.wildcard = true;
                    foreach (var p in defaults) {
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