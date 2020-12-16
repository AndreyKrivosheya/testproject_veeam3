using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace compressor.Common.Threading
{
    abstract class CustomThreadPoolWorkload
    {
        public CustomThreadPoolWorkload(CancellationTokenSource cancellationTokenSource)
        {
            this.CancellationTokenSource = cancellationTokenSource;
        }

        readonly CancellationTokenSource CancellationTokenSource;

        public void Cancel()
        {
            if(this.CancellationTokenSource != null)
            {
                if(!this.CancellationTokenSource.IsCancellationRequested)
                {
                    this.CancellationTokenSource.Cancel();
                }
            }
        }

        public abstract void Invoke();
    }
}