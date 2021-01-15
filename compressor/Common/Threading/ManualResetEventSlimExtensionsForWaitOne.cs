using System.Threading;

namespace compressor.Common.Threading
{
    static class ManualResetEventSlimExtensionsForWaitOne
    {
        public static void WaitOne(this ManualResetEventSlim manualResetEvent)
        {
            manualResetEvent.Wait();
        }

        public static void WaitOne(this ManualResetEventSlim manualResetEvent, CancellationToken cancellationToken)
        {
            manualResetEvent.Wait(cancellationToken);
        }
    }
}