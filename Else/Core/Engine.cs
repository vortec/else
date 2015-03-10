using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using wpfmenu.Core.Plugins;
using wpfmenu.Views;

namespace wpfmenu.Core
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

            if (!Query.Empty) {
                foreach (var p in _plugins) {
                    foreach (var c in p.Providers) {
                        if (c.IsInterested != null) {
                            var x = c.IsInterested(Query);
                            if (x == ProviderInterest.Exclusive) {
                                exclusive.Add(c);
                            }
                            if (x == ProviderInterest.Shared) {
                                shared.Add(c);
                            }
                        }
                    }
                }
                if (exclusive.Any()) {
                    foreach (var c in exclusive) {
                        ResultsList.AddRange(c.Query(Query));
                    }
                }
                else {
                    if (shared.Any()) {
                        foreach (var c in shared) {
                            ResultsList.AddRange(c.Query(Query));
                        }
                    }
                }
            }
        }
    }
}