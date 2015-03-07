using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
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
        List<Plugin> _plugins;
        /// <summary>
        /// Parsed version of the current query.
        /// </summary>
        Model.QueryInfo _info = new Model.QueryInfo();

        public LauncherWindow LauncherWindow;

        public Engine(LauncherWindow launcherWindow) {
            LauncherWindow = launcherWindow;
            // load plugins
            _plugins = new List<Plugin>{
                //new Plugins.Programs(),
                new Web(),
                new Programs(),
                new Math()
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
            _info.Parse(query);

            var exclusive = new List<Plugin>();
            var shared = new List<Plugin>();
            
            // scan plugins and find any that are interested in the query
            if (!_info.Empty) {
                foreach (var p in _plugins) {
                    var interest = p.IsPluginInterested(_info);

                    if (interest == PluginInterest.Exclusive) {
                        exclusive.Add(p);
                    }
                    else if (interest == PluginInterest.Shared) {
                        shared.Add(p);
                    }
                }
            
                // exclusive matches require exclusive control
                if (exclusive.Any()) {
                    foreach (var p in exclusive) {
                        ResultsList.AddRange(p.Query(_info));
                    }
                }
                else {
                    if (shared.Any()) {
                        foreach (var p in shared) {
                            ResultsList.AddRange(p.Query(_info));
                        }
                    }
                    // if no results were provided by the plugins, change query to NoPartialMathces=true and requery plugins with MatchAll=true
                    if (!ResultsList.Any()) {
                        _info.NoPartialMatches = true;
                        foreach (var p in _plugins.Where(p => p.MatchAll)) {
                            ResultsList.AddRange(p.Query(_info));
                        }
                    }
                }
            }
        }
    }
}