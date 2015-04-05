using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Else.Extensibility
{
    public class BaseProvider
    {
        public Func<Query, ProviderInterest> _IsInterestedFunc;
        public Func<Query, CancellationToken, List<Result>> _QueryFunc;
    }
}
