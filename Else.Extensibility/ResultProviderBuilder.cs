using System;
using System.Collections.Generic;
using System.Threading;

namespace Else.Extensibility
{
    public class ResultProviderBuilder : ResultProvider
    {
        public ResultProviderBuilder IsInterested(Func<Query, ProviderInterest> isInterestedFunc)
        {
            IsInterestedFunc = isInterestedFunc;
            return this;
        }

        public ResultProviderBuilder Query(Func<Query, CancellationToken, List<Result>> queryFunc)
        {
            QueryFunc = queryFunc;
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

        public ResultProviderBuilder MatchAll(bool matchAll = true)
        {
            _matchAll = matchAll;
            return this;
        }
    }
}