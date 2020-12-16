using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace compressor.Common.Threading
{
    class CustomThreadPoolWorkloadWithoutArguments : CustomThreadPoolWorkload
    {
        public CustomThreadPoolWorkloadWithoutArguments(CancellationTokenSource cancellationTokenSource, Action workload) : base(cancellationTokenSource)
        {
            this.Workload = workload;
        }

        readonly Action Workload;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Invoke()
        {
            this.Workload();
        }
    }
}