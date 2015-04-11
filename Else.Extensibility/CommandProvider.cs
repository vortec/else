using System;
using System.Collections.Generic;

namespace Else.Extensibility
{
    /// <summary>
    /// A simple Keyword command (with optional arguments)
    /// </summary>
    public class CommandProvider : BaseProvider
    {
        private readonly IAppCommands _appCommands;
        protected bool _fallback;
        protected string _icon;
        protected string _keyword;
        protected Action<Query> _launch;
        protected bool _requiresArguments;
        protected string _subTitle;
        protected string _title;

        public CommandProvider(IAppCommands appCommands)
        {
            _appCommands = appCommands;
            QueryFunc = (query, cts) =>
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
                    if (!string.IsNullOrEmpty(result.Title)) {
                        result.Title = result.Title.Replace("{arguments}", argSub);
                    }
                    if (!string.IsNullOrEmpty(result.SubTitle)) {
                        result.SubTitle = result.SubTitle.Replace("{arguments}", argSub);
                    }
                }

                results.Add(result);
                return results;
            };
            IsInterestedFunc = query =>
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
    }
}