using System.Threading;

using compressor.Common.Threading;

namespace compressor.Processor.Utils
{
    static class WaitHandleExtensionsForWaitOneAndDispose
    {
        public static void WaitOneAndDispose(this WaitHandle waitHandle)
        {
            using(waitHandle)
            {
                waitHandle.WaitOne();
            }
        }

        public static void WaitOneAndDispose(this WaitHandle waitHandle, CancellationToken cancellationToken)
        {
            using(waitHandle)
            {
                waitHandle.WaitOne(cancellationToken);
            }
        }
    }
}