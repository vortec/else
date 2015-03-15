using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Else.Core.Plugins;
using Else.DataTypes;
using Else.Model;
using Math = Else.Core.Plugins.Math;
using SystemCommands = Else.Core.Plugins.SystemCommands;


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
        public BindingResultsList ResultsList = new BindingResultsList();
        /// <summary>
        /// Activated plugins.
        /// </summary>
        List<Plugin> _plugins;
        /// <summary>
        /// Parsed version of the current query.
        /// </summary>
        public Query Query = new Query();

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Stores the raw string of the last query successfully executed.
        /// </summary>
        private string _lastQuery;


        public Engine() {
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
                p.Setup();
            }
        }

        /// <summary>
        /// A plugin has requested that we execute the query again (perhaps it has different results for us)
        /// </summary>
        public void RequestUpdate()
        {
            ExecuteQuery(_lastQuery);
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
        private async void ExecuteQuery(string query)
        {
            _lastQuery = query;
            Query.Parse(query);
            
            // todo: consider removing exclusive
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

                if (_cancellationTokenSource != null) {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }

                // create a new cancellation token 
                _cancellationTokenSource = new CancellationTokenSource();

                try {
                    // store results for this query temporarily before adding to ResultsList, in case the query is cancelled
                    var queryResults = new List<Result>();
                    
                    // if we have any exclusive providers, we ignore all other providers
                    if (exclusive.Any()) {
                        queryResults.AddRange(await ProcessProviderQueryAsync(exclusive));
                    }
                    else if (shared.Any()) {
                        queryResults.AddRange(await ProcessProviderQueryAsync(shared));
                    }
                
                    // if there are no results at all, show the fallback providers
                    if (!queryResults.Any()) {
                        queryResults.AddRange(await ProcessProviderQueryAsync(fallback));
                    }
                    
                    ResultsList.Clear();
                    ResultsList.AddRange(queryResults);
                    
                    // trigger refresh of UI that is bound to the ResultsList
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                        ResultsList.BindingRefresh();
                    }));
                    
                }
                catch (OperationCanceledException) { }
            }
        }
        private async Task<List<Result>> ProcessProviderQueryAsync(List<ResultProvider> providers)
        {
            // invoke Query() on each provider, collect the returned Task() objects
            var tasks = providers
                .Where(p => p != null)
                .Select(p => Task.Run(() => p.Query(Query, _cancellationTokenSource.Token)))
                .ToList();

            // process each task as it finishes
            var results = new List<Result>();
            while (tasks.Count > 0) {
                var next = await Task.WhenAny(tasks);
                tasks.Remove(next);
                try {
                    var taskResults = await next;
                    results.AddRange(taskResults);
                }
                catch (Exception) {
                    // todo: improve handling here.  ideally we would ignore results from this plugin
                    Debug.Print("provider failure :|");
                }
            }
            return results;
        }
    }
}