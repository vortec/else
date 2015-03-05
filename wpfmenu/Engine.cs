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
        public Types.BindingResultsList resultsList = new Types.BindingResultsList();
        
        // loaded plugins
        List<Plugins.Plugin> plugins = new List<Plugins.Plugin>();
        Model.QueryInfo info = new Model.QueryInfo();
        
        public Engine() {
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

            Messenger.Default.Register<Messages.Launch>(this, Launch);
        }

        public void Launch(Messages.Launch message)
        {
            //message.result.Launch(info, message.result);
            if (message.result.Launch != null) {
                message.result.Launch();
            }
        }
        
        // process the query when it changes
        public void QueryChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var query = textbox.Text;
            
            resultsList.Clear();
            info.parse(query);
            
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
                                resultsList.Add(r);
                            }
                            showDefaultResults = false;
                        }
                    }
                }
                if (showDefaultResults) {
                    // no plugin found, show wildcard results
                    info.generic = true;
                    foreach (var p in defaults) {
                        var results = p.Query(info);
                        foreach (var r in results) {
                            resultsList.Add(r);
                        }
                    }
                }
            }
            Messenger.Default.Send(new Messages.ResultsUpdated());
        }
    }
}