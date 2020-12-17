using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace compressor.Common.Threading
{
    class CustomThreadPoolWorkloadWithArguments<T> : CustomThreadPoolWorkload
    {
        public CustomThreadPoolWorkloadWithArguments(Action<T> workload, T arg)
        {
            this.Workload = workload;
            this.Arg = arg;
        }

        readonly Action<T> Workload;
        readonly T Arg;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Invoke()
        {
            this.Workload(this.Arg);
        }
    }

    class CustomThreadPoolWorkloadWithArguments<T1, T2> : CustomThreadPoolWorkload
    {
        public CustomThreadPoolWorkloadWithArguments(Action<T1, T2> workload, T1 arg1, T2 arg2)
        {
            this.Workload = workload;
            this.Arg1 = arg1;
            this.Arg2 = arg2;
        }

        readonly Action<T1, T2> Workload;
        readonly T1 Arg1;
        readonly T2 Arg2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Invoke()
        {
            this.Workload(this.Arg1, this.Arg2);
        }
    }

    class CustomThreadPoolWorkloadWithArguments<T1, T2, T3> : CustomThreadPoolWorkload
    {
        public CustomThreadPoolWorkloadWithArguments(Action<T1, T2, T3> workload, T1 arg1, T2 arg2, T3 arg3)
        {
            this.Workload = workload;
            this.Arg1 = arg1;
            this.Arg2 = arg2;
            this.Arg3 = arg3;
        }

        readonly Action<T1, T2, T3> Workload;
        readonly T1 Arg1;
        readonly T2 Arg2;
        readonly T3 Arg3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Invoke()
        {
            this.Workload(this.Arg1, this.Arg2, this.Arg3);
        }
    }
}