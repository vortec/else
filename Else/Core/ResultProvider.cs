using System;
using System.Collections.Generic;
using System.Threading;
using Else.Lib;
using Else.Model;

namespace Else.Core
{
    /// <summary>
    /// Provider interest for the current query.
    /// </summary>
    public enum ProviderInterest {
        /// <summary>
        /// Plugin has no interest in providing results for the query.
        /// </summary>
        None,
        /// <summary>
        /// Plugin shares control over results with other plugins.
        /// </summary>
        Shared,
        /// <summary>
        /// The fallback
        /// </summary>
        Fallback,
        /// <summary>
        /// Plugin demands exclusive control over the results for the query.
        /// </summary>
        Exclusive,
    }

    /// <summary>
    /// Base ResultProvider, can be used directly or derived from.
    /// </summary>
    public class ResultProvider
    {
        public string Keyword;
        /// <summary>
        /// Provides results with and without a token.
        /// </summary>
        public bool MatchAll;
        /// <summary>
        /// todo: Provides results when no other plugins provide results. This may become configurable in the settings UI.
        /// </summary>
        public bool Fallback;
        public Func<Query, ProviderInterest> IsInterested;
        public Func<Query, CancellationToken, List<Result>> Query;
        
        public ResultProvider()
        {
            IsInterested = query => {
                 if (!query.Keyword.IsEmpty()) {
                    if (query.KeywordComplete && Keyword == query.Keyword) {
                        return ProviderInterest.Exclusive;
                    }
                    if (!query.KeywordComplete && Keyword.StartsWith(query.Keyword)) {
                        return ProviderInterest.Shared;
                    }
                }
                if (Fallback) {
                    return ProviderInterest.Fallback;
                }
                if (MatchAll) {
                    return ProviderInterest.Shared;
                }
                return ProviderInterest.None;
            };
        }
    };
}
