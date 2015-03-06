using System.Collections.Generic;
using System.Linq;

namespace wpfmenu.Plugins
{

    /// <summary>
    /// The type of match, determined from the query and the plugin tokens.
    /// </summary>
    public enum TokenMatch {
        /// <summary>
        /// No matches
        /// </summary>
        None,
        /// <summary>
        /// The plugin matches all queries (<see cref="Plugin.MatchAll"/>)
        /// </summary>
        All,
        /// <summary>
        /// Partial match, e.g. query "goo" partially matches "google"
        /// </summary>
        Partial,
        /// <summary>
        /// Exact match, e.g. query "google" exactly matches "google"
        /// </summary>
        Exact,
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
        /// Checks the plugin token against the <see cref="Model.QueryInfo"/> and returns the match type.
        /// </summary>
        public TokenMatch CheckToken(Model.QueryInfo info)
        {
            if (Tokens.Contains(info.Token)) {
                return TokenMatch.Exact;
            }
            if (!info.TokenComplete && Tokens.Any(token => token.StartsWith(info.Token))) {
                return TokenMatch.Partial;
            }
            if (MatchAll) {
                return TokenMatch.All;
            }
            return TokenMatch.None;
        }
    }
}
