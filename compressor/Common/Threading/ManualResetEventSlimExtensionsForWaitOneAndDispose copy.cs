using System.Threading;

namespace compressor.Common.Threading
{
    static class ManualResetEventSlimExtensionsForWaitOneAndDispose
    {
        public static void WaitOneAndDispose(this ManualResetEventSlim manualResetEvent)
        {
            using(manualResetEvent)
            {
                manualResetEvent.WaitOne();
            }
        }

        public static void WaitOneAndDispose(this ManualResetEventSlim manualResetEvent, CancellationToken cancellationToken)
        {
            using(manualResetEvent)
            {
                manualResetEvent.WaitOne(cancellationToken);
            }
        }
    }
}