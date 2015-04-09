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
        private readonly CancellationTokenSource _cts;

        public InterAppDomainCancellable()
        {
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }

        public void Cancel()
        {
            _cts.Cancel();
        }

        CancellationToken ITokenSource.Token => _cts.Token;
    }
}