
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Else.Core.Plugins;
using Else.Views;
using Math = Else.Core.Plugins.Math;
using TextBox = System.Windows.Controls.TextBox;

namespace Else.Core
{
    /// <summary>
    /// Handles parsing of the query and querying plugins for results.
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// The results list
        /// </summary>
        public DataTypes.BindingResultsList ResultsList = new DataTypes.BindingResultsList();
        /// <summary>
        /// Activated plugins.
        /// </summary>
        List<Plugin> _plugins;
        /// <summary>
        /// Parsed version of the current query.
        /// </summary>
        public Model.Query Query = new Model.Query();

        public LauncherWindow LauncherWindow;

        public Engine(LauncherWindow launcherWindow) {
            LauncherWindow = launcherWindow;
            // load plugins
            _plugins = new List<Plugin>{
                new GoogleSuggest(),
                new Web(),
                new Programs(),
                new Math(),
                new SystemCommands()
            };
            // setup plugins
            foreach (var p in _plugins) {
                p.Init(this);
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
            Query.Parse(query);
            
            var exclusive = new List<ResultProvider>();
            var shared = new List<ResultProvider>();
            var fallback = new List<ResultProvider>();

            if (!Query.Empty) {
                // determine which providers are able to respond to this query
                foreach (var p in _plugins) {
                    foreach (var c in p.Providers) {
                        if (c.IsInterested != null) {
                            // sort them into groups
                            var x = c.IsInterested(Query);
                            if (x == ProviderInterest.Exclusive) {
                                exclusive.Add(c);
                            }
                            else if (x == ProviderInterest.Shared) {
                                shared.Add(c);
                            }
                            else if (x == ProviderInterest.Fallback) {
                                fallback.Add(c);
                            }
                        }
                    }
                }

                var fetchResultsAsync = new Action<List<ResultProvider>>(async providers => {
                    var tasks = providers.Select(p => p.Query(Query)).ToList();
                    while (tasks.Count > 0) {
                        var next = await Task.WhenAny(tasks);
                        tasks.Remove(next);
                        var results = await next;
                        ResultsList.AddRange(results);
                    }
                });

                // if we have any exclusive providers, we ignore all other providers
                if (exclusive.Any()) {
                    fetchResultsAsync(exclusive);
                }
                else {
                    // show shared providers
                    if (shared.Any()) {
                        fetchResultsAsync(shared);
                    }
                }

                // if there are no results at all, show the fallback providers
                if (!ResultsList.Any()) {
                    fetchResultsAsync(fallback);
                }
            }
        }
    }
}