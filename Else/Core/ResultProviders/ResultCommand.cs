using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Else.Extensions;
using Else.Model;

namespace Else.Core.ResultProviders
{
    /// <summary>
    /// Helper class for providing results for a single command, with optional arguments support.
    /// </summary>
    public class ResultCommand : ResultProvider
    {
        public string Title;
        public string SubTitle;
        public Action<Query> Launch;
        public Lazy<BitmapSource> Icon;
        public bool RequiresArguments;
        public ResultCommand()
        {
            Query = (query, cts) => {
                var results = new List<Result>();
                
                var result = new Result{
                    Title = Title,
                    SubTitle = SubTitle,
                    Icon = Icon,
                    Launch = HandleLaunch
                };

                // attempt to replace tokens in result Title..
                if (RequiresArguments) {
                    var arguments = "";
                    // check if keyword was matched
                    if (Keyword.StartsWith(query.Keyword)) {
                        arguments = query.Arguments;
                    }
                    else if (Fallback) {
                        arguments = query.Raw;
                    }
                    var argSub = arguments.IsEmpty() ? "..." : arguments;
                    result.Title = result.Title.Replace("{arguments}", argSub);
                }

                results.Add(result);
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
                    //AppCommands.RewriteQuery(Keyword + ' ');
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
