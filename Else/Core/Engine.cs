using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Else.DataTypes;
using Else.Model;
using Else.Extensibility;

namespace Else.Core
{
    /// <summary>
    /// Handles parsing of the query and querying plugins for results.
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Activated plugins
        /// </summary>
        public List<Plugin> Plugins => _pluginManager.Plugins;

        /// <summary>
        /// The cancellation token of the currently executing query.
        /// </summary>
        private CancellationTokenSource _cancelTokenSource;

        /// <summary>
        /// Stores the raw string of the last query successfully executed.
        /// </summary>
        private string _lastQuery;

        /// <summary>
        /// Parsed version of the current query.
        /// </summary>
        public Query Query = new Query();

        /// <summary>
        /// The results list
        /// </summary>
        public BindingResultsList ResultsList = new BindingResultsList();

        private readonly PluginManager _pluginManager;

        public Engine(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        /// <summary>
        /// A plugin has requested that we execute the query again (perhaps it has different results for us)
        /// </summary>
        public void RequestUpdate()
        {
            BeginQuery(_lastQuery);
        }

        /// <summary>
        /// Called when [query changed].
        /// </summary>
        public void OnQueryChanged(string query)
        {
            BeginQuery(query);
        }

        public async void BeginQuery(string query)
        {
            // parse the query
            Query.Parse(query);

            // if a query is already running, request cancellation
            if (_cancelTokenSource != null) {
                _cancelTokenSource.Cancel(true);
                //_cancelTokenSource.Dispose();
                //_cancelTokenSource = null;
            }

            // empty query, remove existing results
            if (Query.Empty) {
                ResultsList.Clear();
                ResultsList.BindingRefresh();
                return;
            }

            // create a new cancellation token 
            _cancelTokenSource = new CancellationTokenSource();

            // execute the query in a new thread
            try {
                await Task.Factory.StartNew(async () => { await ExecuteQuery(query); }, _cancelTokenSource.Token);
            }
            catch (TaskCanceledException) {
                Debug.Print("caught exception");
            }
        }

        /// <summary>
        /// Update ResultsList by querying plugins.
        /// </summary>
        /// <param name="query">The query.</param>
        private async Task ExecuteQuery(string query)
        {
            // determine which providers are able to respond to this query, and sort them into groups
            var exclusive = new List<BaseProvider>(); // todo: consider removing exclusive
            var shared = new List<BaseProvider>();
            var fallback = new List<BaseProvider>();
            foreach (var p in Plugins) {
                foreach (var c in p.Providers) {
                    if (c._IsInterestedFunc != null) {
                        var x = c._IsInterestedFunc(Query);
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

            // create a new cancellation token 
            _cancelTokenSource = new CancellationTokenSource();

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

                // query successful, show the results
                ResultsList.Clear();
                ResultsList.AddRange(queryResults);
                _lastQuery = query;

                // trigger refresh of UI that is bound to the ResultsList
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => { ResultsList.BindingRefresh(); }));
            }
            catch (OperationCanceledException) {
            }
        }

        private async Task<List<Result>> ProcessProviderQueryAsync(List<BaseProvider> providers)
        {
            // invoke Query() on each provider, collect the returned Task() objects
            var tasks = new List<Task<List<Result>>>();
            foreach (var provider in providers) {
                if (provider != null) {
                    try {
                        var task = Task.Factory.StartNew(() => provider._QueryFunc(Query, _cancelTokenSource.Token), _cancelTokenSource.Token);
                        tasks.Add(task);
                    }
                    catch {
                        Debug.Print("Failed to start provider");
                    }
                }
            }

            // process each task as it finishes
            var results = new List<Result>();
            while (tasks.Count > 0) {
                var next = await Task.WhenAny(tasks);
                tasks.Remove(next);
                try {
                    var taskResults = await next;
                    results.AddRange(taskResults);
                }
                catch (OperationCanceledException) {
                    // rethrow this exception.
                    throw;
                }
                catch (Exception) {
                    Debug.Print("provider failure :|");
                }
            }
            return results;
        }
    }
}