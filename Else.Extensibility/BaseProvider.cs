using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;

namespace Else.Extensibility
{
    public class BaseProvider : MarshalByRefObject
    {
        private readonly ClientSponsor _sponsor = new ClientSponsor();
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

        public InterAppDomainCancellable GetCancellable()
        {
            var cancellable = new InterAppDomainCancellable();
            var lease = (ILease) cancellable.InitializeLifetimeService();
            lease?.Register(_sponsor);
            return cancellable;
        }
    }
}