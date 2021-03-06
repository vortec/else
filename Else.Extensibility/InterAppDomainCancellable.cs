﻿using System;
using System.Threading;

namespace Else.Extensibility
{
    public interface ITokenSource
    {
        CancellationToken Token { get; }
        void Dispose();
        void Cancel();
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

        CancellationToken ITokenSource.Token => _cts.Token;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Cancel()
        {
            _cts.Cancel();
        }
    }
}