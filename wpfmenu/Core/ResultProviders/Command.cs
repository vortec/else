using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using wpfmenu.Lib;
using wpfmenu.Model;
using wpfmenu.Views;

namespace wpfmenu.Core.ResultProviders
{
    /// <summary>
    /// Helper class for providing results for a single command, with optional arguments support.
    /// </summary>
    public class Command : ResultProvider
    {
        public string Title;
        public string SubTitle;
        public Action<Query> Launch;
        public BitmapImage Icon;
        public bool RequiresArguments;
        public Command()
        {
            Query = query => {
                var results = new List<Result>();
                
                if (Keyword.StartsWith(query.Keyword)) {
                    var result = new Result{
                        Title = Title,
                        Icon = Icon,
                        Launch = HandleLaunch
                    };
                    if (RequiresArguments) {
                        var argSub = query.Arguments.IsEmpty() ? "..." : query.Arguments;
                        result.Title = result.Title.Replace("{arguments}", argSub);
                    }
                    results.Add(result);
                }
                return results;
            };
        }

        /// <summary>
        /// Wraps the command Launch method.  If the keyword is complete, Launch is invoked, otherwise rewrite the query to autocomplete the keyword.
        /// </summary>
        private void HandleLaunch(Query query)
        {
            if (RequiresArguments) {
                if (!query.KeywordComplete || query.Arguments.IsEmpty()) {
                    // auto complete the query
                    Globals.PluginCommands.RewriteQuery(Keyword + ' ');
                }
                else {
                    Launch(query);
                }
            }
            else {
                Launch(query);
            }
        }
    };
}
