using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace compressor.Common.Threading
{
    abstract class CustomThreadPoolWorkload
    {
        public CustomThreadPoolWorkload()
        {
        }

        public abstract void Invoke();
    }
}