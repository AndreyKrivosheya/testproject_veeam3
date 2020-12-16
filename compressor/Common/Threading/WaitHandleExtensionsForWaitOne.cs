using System;
using System.Linq;
using System.Threading;

namespace compressor.Common.Threading
{
    static class WaitHandleExtensionsForWaitOne
    {
        public static void WaitOne(this WaitHandle waitHandle, CancellationToken cancellationToken)
        {
            var waitHandles = new[] { waitHandle, cancellationToken.WaitHandle };
            var waitFinishedForIndex = WaitHandle.WaitAny(waitHandles, Timeout.Infinite);
            if(waitFinishedForIndex != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}