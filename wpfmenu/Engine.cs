﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using wpfmenu.Plugins;

namespace wpfmenu
{

    /// <summary>
    /// Handles parsing of the query and querying plugins for results.
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// The results list
        /// </summary>
        public Types.BindingResultsList ResultsList = new Types.BindingResultsList();
        /// <summary>
        /// Activated plugins.
        /// </summary>
        List<Plugins.Plugin> _plugins = new List<Plugins.Plugin>();
        /// <summary>
        /// Parsed version of the current query.
        /// </summary>
        Model.QueryInfo _info = new Model.QueryInfo();

        public Engine() {
            // load plugins
            _plugins = new List<Plugins.Plugin>{
                //new Plugins.Programs(),
                new Plugins.Web(),
                new Plugins.Programs()
            };
            // setup plugins
            foreach (var p in _plugins) {
                p.Setup();
            }
        }

        /// <summary>
        /// Called when [query changed].
        /// </summary>
        public void OnQueryChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox != null) {
                var query = textbox.Text;
                ExecuteQuery(query);
            }
        }
        /// <summary>
        /// Update ResultsList by querying plugins.
        /// </summary>
        /// <param name="query">The query.</param>
        private void ExecuteQuery(string query)
        {
            ResultsList.Clear();
            _info.Parse(query);
            
            if (!_info.Empty) {
                // query plugins
                foreach (var p in _plugins) {
                    var match = p.CheckToken(_info);
                    if (match > TokenMatch.None) {
                        var results = p.Query(_info);
                        if (results.Any()) {
                            foreach (var r in results) {
                                ResultsList.Add(r);
                            }
                        }
                    }
                }
                
                // if no results were provided by the plugins, change query to NoPartialMathces=true and requery plugins with MatchAll=true
                if (!ResultsList.Any()) {
                    _info.NoPartialMatches = true;
                    foreach (var p in _plugins.Where(p => p.MatchAll)) {
                        var results = p.Query(_info);
                        foreach (var r in results) {
                            ResultsList.Add(r);
                        }
                    }
                }
            }

            Messenger.Default.Send(new Messages.ResultsUpdated());
        }
    }
}