using System;
using System.Collections.Generic;

namespace Else.Extensibility
{
    public class BaseProvider : MarshalByRefObject
    {
        public Func<Query, ProviderInterest> IsInterestedFunc;
        public Func<Query, ITokenSource, List<Result>> QueryFunc;

        public ProviderInterest ExecuteIsInterestedFunc(Query query)
        {
            if (IsInterestedFunc != null) {
                return IsInterestedFunc(query);
            }
            return ProviderInterest.None;
        }

        public List<Result> ExecuteQueryFunc(Query query, ITokenSource cancelToken)
        {
            if (QueryFunc != null) {
                return QueryFunc(query, cancelToken);
            }
            return new List<Result>();
        }
    }
}