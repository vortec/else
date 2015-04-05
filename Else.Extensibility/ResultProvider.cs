namespace Else.Extensibility
{
    public class ResultProvider : BaseProvider
    {
        protected bool _isFallback;
        protected string _keyword;
        protected bool _matchAll;

        public ResultProvider()
        {
            IsInterestedFunc = query =>
            {
                if (!string.IsNullOrEmpty(query.Keyword) && !string.IsNullOrEmpty(_keyword)) {
                    if (query.KeywordComplete && _keyword == query.Keyword) {
                        return ProviderInterest.Exclusive;
                    }
                    if (!query.KeywordComplete && query.Keyword.StartsWith(_keyword)) {
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
    }
}