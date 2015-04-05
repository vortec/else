using System;
using System.Collections.Generic;
using System.Threading;

namespace Else.Extensibility
{
    public class BaseProvider
    {
        public Func<Query, ProviderInterest> IsInterestedFunc;
        public Func<Query, CancellationToken, List<Result>> QueryFunc;
    }
}
