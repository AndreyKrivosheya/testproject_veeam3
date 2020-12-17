using System;
using System.Linq;
using System.Threading;

namespace compressor.Common.Threading
{
    static class WaitHandleExtensionsForWaitOne
    {
        public static void WaitOne(this WaitHandle waitHandle, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            WaitHandle.WaitAny(new[] { waitHandle, cancellationToken.WaitHandle }, Timeout.Infinite);

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}