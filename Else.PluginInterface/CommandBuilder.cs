using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Else.PluginInterface
{
    public class CommandBuilder : BaseProvider
    {
        private readonly IAppCommands _appCommands;
        private bool _fallback;
        private Lazy<BitmapSource> _icon;
        private string _keyword;
        private Action<Query> _launch;
        private bool _requiresArguments;
        private string _subTitle;
        private string _title;

        /// <summary>
        /// Builds a simple Keyword command (with optional arguments)
        /// </summary>
        /// <param name="appCommands"></param>
        public CommandBuilder(IAppCommands appCommands)
        {
            _appCommands = appCommands;
            _QueryFunc = (query, cts) =>
            {
                var results = new List<Result>();

                var result = new Result
                {
                    Title = _title,
                    SubTitle = _subTitle,
                    Icon = _icon,
                    Launch = HandleLaunch
                };

                // attempt to replace tokens in result Title..
                if (_requiresArguments) {
                    var arguments = "";
                    // check if keyword was matched
                    if (_keyword.StartsWith(query.Keyword)) {
                        arguments = query.Arguments;
                    }
                    else if (_fallback) {
                        arguments = query.Raw;
                    }
                    var argSub = string.IsNullOrEmpty(arguments) ? "..." : arguments;
                    result.Title = result.Title.Replace("{arguments}", argSub);
                }

                results.Add(result);
                return results;
            };
            _IsInterestedFunc = query =>
            {
                if (!string.IsNullOrEmpty(query.Keyword)) {
                    if (query.KeywordComplete && _keyword == query.Keyword) {
                        return ProviderInterest.Exclusive;
                    }
                    if (!query.KeywordComplete && _keyword.StartsWith(query.Keyword)) {
                        return ProviderInterest.Shared;
                    }
                }
                if (_fallback) {
                    return ProviderInterest.Fallback;
                }
//                if (_matchAll) {
//                    return ProviderInterest.Shared;
//                }
                return ProviderInterest.None;
            };
        }

        /// <summary>
        /// Wraps the command Launch method.  If the keyword is complete, Launch is invoked, otherwise rewrite the query to autocomplete the keyword.
        /// </summary>
        private void HandleLaunch(Query query)
        {
            if (_requiresArguments) {
                if (!query.KeywordComplete || string.IsNullOrEmpty(query.Arguments)) {
                    // auto complete the query
                    _appCommands.RewriteQuery(_keyword + ' ');
                }
                else {
                    _launch(query);
                }
            }
            else {
                _launch(query);
            }
        }

        /// <summary>
        /// The result title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public CommandBuilder Title(string title)
        {
            _title = title;
            return this;
        }
        /// <summary>
        /// The result subtitle
        /// </summary>
        /// <param name="subTitle"></param>
        /// <returns></returns>
        public CommandBuilder Subtitle(string subTitle)
        {
            _subTitle = subTitle;
            return this;
        }

        /// <summary>
        /// The result action
        /// </summary>
        /// <param name="launch"></param>
        /// <returns></returns>
        public CommandBuilder Launch(Action<Query> launch)
        {
            _launch = launch;
            return this;
        }

        /// <summary>
        /// The result icon
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public CommandBuilder Icon(Lazy<BitmapSource> icon)
        {
            _icon = icon;
            return this;
        }

        /// <summary>
        /// The keyword required to trigger this command
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public CommandBuilder Keyword(string keyword)
        {
            _keyword = keyword;
            return this;
        }

        /// <summary>
        /// This command is a fallback command (provides a result when no other plugin has results).
        /// </summary>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public CommandBuilder Fallback(bool fallback = true)
        {
            _fallback = fallback;
            return this;
        }

        /// <summary>
        /// This command requires arguments.
        /// </summary>
        /// <param name="requiresArguments"></param>
        /// <returns></returns>
        public CommandBuilder RequiresArguments(bool requiresArguments = true)
        {
            _requiresArguments = requiresArguments;
            return this;
        }
    }
}