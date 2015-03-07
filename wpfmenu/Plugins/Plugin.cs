using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace wpfmenu.Plugins
{

    /// <summary>
    /// Plugin interest on the current query.
    /// </summary>
    public enum PluginInterest {
        /// <summary>
        /// Plugin has no interest in providing results for the query.
        /// </summary>
        None,
        /// <summary>
        /// Plugin shares control over results with other plugins.
        /// </summary>
        Shared,
        /// <summary>
        /// Plugin demands exclusive control over the results for the query.
        /// </summary>
        Exclusive,
    };

    /// <summary>
    /// Base plugin.  This class is not instantiated directly, rather all plugins derive from it.
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// Queries the plugin for results.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List of <see cref="Model.Result" /> to be displayed on the launcher</returns>
        abstract public List<Model.Result> Query(Model.QueryInfo query);
        /// <summary>
        /// Tokens that are handled by this plugin
        /// </summary>
        public List<string> Tokens = new List<string>();
        /// <summary>
        /// Whether this plugin provides results regardless of token match
        /// </summary>
        public bool MatchAll;
        public Engine Engine;
        
        /// <summary>
        /// Initializes this instance with dependancies.
        /// </summary>
        public void Init(Engine engine)
        {
            Engine = engine;
        }
        
        /// <summary>
        /// Plugin setup
        /// </summary>
        abstract public void Setup();

        


        /// <summary>
        /// Default method to determine if the plugin is interested in a query.
        /// </summary>
        /// <param name="info">The query information.</param>
        /// <returns></returns>
        public virtual PluginInterest IsPluginInterested(Model.QueryInfo info)
        {
            if (info.TokenComplete && Tokens.Contains(info.Token)) {
                return PluginInterest.Exclusive;
            }
            if (!info.TokenComplete && Tokens.Any(token => token.StartsWith(info.Token))) {
                return PluginInterest.Shared;
            }
            if (MatchAll) {
                return PluginInterest.Shared;
            }
            return PluginInterest.None;
        }

        
    }
}
