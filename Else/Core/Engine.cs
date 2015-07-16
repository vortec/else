﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Autofac.Extras.NLog;
using Else.DataTypes;
using Else.Extensibility;

namespace Else.Core
{
    /// <summary>
    /// Handles parsing of the query and querying plugins for results.
    /// </summary>
    public class Engine
    {
        /// <summary>
        ///  lock for synchronization of ResultsList
        /// </summary>
        private static readonly object SyncLock = new object();

        private readonly ILogger _logger;
        private readonly PluginManager _pluginManager;

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

        public Engine(ILogger logger, PluginManager pluginManager)
        {
            _logger = logger;
            _pluginManager = pluginManager;
            BindingOperations.EnableCollectionSynchronization(ResultsList, SyncLock);
        }

        /// <summary>
        /// Activated plugins
        /// </summary>
        public List<Plugin> Plugins => _pluginManager.Plugins;

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
                lock (ResultsList) {
                    ResultsList.Clear();
                    ResultsList.BindingRefresh();
                }
                return;
            }

            // create a new cancellation token 
            _cancelTokenSource = new CancellationTokenSource();

            // execute the query in a new thread
            try {
                await Task.Factory.StartNew(async () => { await ExecuteQuery(query); }, _cancelTokenSource.Token);
            }
            catch (TaskCanceledException) {
            }
            catch (AggregateException) {
                // we already log these exceptions in ProcessProviderQueryAsync()...
                // foreach (var e in ae.Flatten().InnerExceptions) {
                //     _logger.Error("Plugin query threw an exception", e);
                // }
            }
        }

        /// <summary>
        /// Update ResultsList by querying plugins.
        /// </summary>
        /// <param name="query">The query.</param>
        private async Task ExecuteQuery(string query)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (ResultsList) {
                    ResultsList.Clear();
                }
            }));
            // determine which providers are able to respond to this query, and sort them into groups
            var exclusive = new List<IProvider>();
            var shared = new List<IProvider>();
            var fallback = new List<IProvider>();
            foreach (var p in Plugins) {
                foreach (var c in p.Providers) {
                    var interest = c.ExecuteIsInterestedFunc(Query);
                    switch (interest) {
                        case ProviderInterest.Exclusive:
                            exclusive.Add(c);
                            break;
                        case ProviderInterest.Fallback:
                            fallback.Add(c);
                            break;
                        case ProviderInterest.Shared:
                            shared.Add(c);
                            break;
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
                if (_cancelTokenSource.IsCancellationRequested) {
                    return;
                }
                // query successful, show the results
                lock (ResultsList) {
                    ResultsList.Clear();
                    ResultsList.AddRange(queryResults);
                }
                _lastQuery = query;

                // trigger refresh of UI that is bound to the ResultsList
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lock (ResultsList) {
                        ResultsList.BindingRefresh();
                    }
                }));
            }
            catch (OperationCanceledException) {
            }
        }

        private async Task<List<Result>> ProcessProviderQueryAsync(List<IProvider> providers)
        {
            // invoke Query() on each provider, collect the returned Task() objects
            var tasks = new List<Task<List<Result>>>();
            foreach (var provider in providers) {
                if (provider != null) {
                    var task = Task.Factory.StartNew(() =>
                    {
                        // create a cancellable that is remotable
                        var cancellable = new InterAppDomainCancellable();

                        // connect our local cancellation token with the remote one
                        _cancelTokenSource.Token.Register(() =>
                        {
                            cancellable.Cancel();
                            cancellable.Dispose();
                        });

                        // query the provider and pass the remotable cancellable
                        return provider.ExecuteQueryFunc(Query, cancellable);
                    }, _cancelTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    tasks.Add(task);
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
                catch (Exception e) {
                    _logger.Error("Plugin query threw an exception: {0}", e.Message);
                }
            }
            return results;
        }
    }
}