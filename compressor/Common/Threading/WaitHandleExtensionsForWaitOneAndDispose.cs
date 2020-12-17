using System.Threading;

namespace compressor.Common.Threading
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