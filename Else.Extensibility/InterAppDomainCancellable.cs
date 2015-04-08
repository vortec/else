using System;
using System.Threading;

namespace Else.Extensibility
{
    public interface ITokenSource
    {
        CancellationToken Token { get; }
    }

    public class InterAppDomainCancellable : MarshalByRefObject, ITokenSource, IDisposable
    {
        private readonly CancellationTokenSource cts;

        public InterAppDomainCancellable()
        {
            cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            cts.Dispose();
        }

        public void Cancel()
        {
            cts.Cancel();
        }

        CancellationToken ITokenSource.Token
        {
            get { return cts.Token; }
        }
    }
}