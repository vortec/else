using System;
using System.Collections.Generic;
using System.Threading;

namespace Else.PluginInterface
{
    public class ResultProviderBuilder : BaseProvider
    {
        private bool _isFallback;
        private string _keyword;
        private bool _matchAll;

        public ResultProviderBuilder()
        {
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
                if (_isFallback) {
                    return ProviderInterest.Fallback;
                }
                if (_matchAll) {
                    return ProviderInterest.Shared;
                }
                return ProviderInterest.None;
            };
        }

        public ResultProviderBuilder IsInterested(Func<Query, ProviderInterest> isInterestedFunc)
        {
            _IsInterestedFunc = isInterestedFunc;
            return this;
        }

        public ResultProviderBuilder Query(Func<Query, CancellationToken, List<Result>> queryFunc)
        {
            _QueryFunc = queryFunc;
            return this;
        }

        public ResultProviderBuilder IsFallback(bool isFallback = true)
        {
            _isFallback = isFallback;
            return this;
        }

        public ResultProviderBuilder Keyword(string keyword)
        {
            _keyword = keyword;
            return this;
        }

        public ResultProviderBuilder MatchAll(bool matchAll)
        {
            _matchAll = matchAll;
            return this;
        }
    }
}
