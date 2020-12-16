using System;
using System.Linq;
using System.Threading;

namespace compressor.Common.Threading
{
    class CustomThreadPool : IDisposable
    {
        public CustomThreadPool(int concurrency)
        {
            concurrency = Math.Max(1, concurrency);

            this.WorkloadsFinishedEvents = new Semaphore[concurrency];
            for(var i = 0; i < concurrency; ++i)
            {
                this.WorkloadsFinishedEvents[i] = new Semaphore(1, 1);
            }
            this.WorkloadsReadyEvents = new AutoResetEvent[concurrency];
            for(var i = 0; i < concurrency; ++i)
            {
                this.WorkloadsReadyEvents[i] = new AutoResetEvent(false);
            }
            this.Workloads = new CustomThreadPoolWorkload[concurrency];

            // spin off thread pool
            this.ThreadsCancellation = new CancellationTokenSource();
            this.Threads = new Thread[concurrency];
            for(var i = 0; i < concurrency; ++i)
            {
                Threads[i] = new Thread((obj) => {
                    var index = (int)obj;
                    
                    var keepRunning = true;
                    while(keepRunning)
                    {
                        switch(WaitHandle.WaitAny(new WaitHandle[] { WorkloadsReadyEvents[index], ThreadsCancellation.Token.WaitHandle }, Timeout.Infinite))
                        {
                            case 1:
                                keepRunning = false;
                                break;
                            case 0:
                            default:
                                try
                                {
                                    var workload = Workloads[index];
                                    if(workload != null)
                                    {
                                        try
                                        {
                                            workload.Invoke();
                                        }
                                        finally
                                        {
                                            Workloads[index] = null;
                                        }
                                    }
                                }
                                finally
                                {
                                    WorkloadsFinishedEvents[index].Release();
                                }
                                break;
                        }
                    }
                });
                Threads[i].IsBackground = true;
                Threads[i].Start(i);
            }
        }

        CancellationTokenSource ThreadsCancellation;
        Thread[] Threads;

        Semaphore[] WorkloadsFinishedEvents;
        AutoResetEvent[] WorkloadsReadyEvents;
        CustomThreadPoolWorkload[] Workloads;

        
        public virtual void Dispose()
        {
            // stop threads
            if(ThreadsCancellation != null)
            {
                if(!ThreadsCancellation.IsCancellationRequested)
                {
                    ThreadsCancellation.Cancel();
                    for(var i = 0; i < Workloads.Length; ++i)
                    {
                        var workload = Workloads[i];
                        if(workload != null)
                        {
                            workload.Cancel();
                        }
                    }
                }
                for(var i = 0; i < Threads.Length; ++i)
                {
                    Threads[i].Join();
                    Threads[i] = null;
                }
                ThreadsCancellation = null;
            }
            // clean up sync objects
            if(WorkloadsFinishedEvents != null)
            {
                for(var i = 0; i < WorkloadsFinishedEvents.Length; ++i)
                {
                    if(WorkloadsFinishedEvents[i] != null)
                    {
                        WorkloadsFinishedEvents[i].Dispose();
                        WorkloadsFinishedEvents[i] = null;
                    }
                }
                WorkloadsFinishedEvents = null;
            }
            if(WorkloadsReadyEvents != null)
            {
                for(var i = 0; i < WorkloadsReadyEvents.Length; ++i)
                {
                    if(WorkloadsReadyEvents[i] != null)
                    {
                        WorkloadsReadyEvents[i].Dispose();
                        WorkloadsReadyEvents[i] = null;
                    }
                }
                WorkloadsReadyEvents = null;
            }
            if(Workloads != null)
            {
                for(var i = 0; i < Workloads.Length; ++i)
                {
                    Workloads[i] = null;
                }
                Workloads = null;
            }
        }

        void Queue(CancellationToken cancellationToken, CustomThreadPoolWorkload workload)
        {
            var waitHandles = Enumerable.Concat(WorkloadsFinishedEvents, new [] { cancellationToken.WaitHandle }).ToArray();
            var idxFreeThread = WaitHandle.WaitAny(waitHandles, Timeout.Infinite);
            if(idxFreeThread < WorkloadsFinishedEvents.Length)
            {
                Workloads[idxFreeThread] = workload;
                WorkloadsReadyEvents[idxFreeThread].Set();
            }
        }
        void Queue(CancellationTokenSource cancellationTokenSource, CustomThreadPoolWorkload workload)
        {
            Queue(cancellationTokenSource == null ? CancellationToken.None : cancellationTokenSource.Token, workload);
        }

        public void Queue(CancellationTokenSource cancellationTokenSource, Action workload)
        {
            Queue(cancellationTokenSource, new CustomThreadPoolWorkloadWithoutArguments(cancellationTokenSource, workload));
        }
        public void Queue(Action workload)
        {
            Queue(default(CancellationTokenSource), workload);
        }
        
        public void Queue<T>(CancellationTokenSource cancellationTokenSource, Action<T> workload, T arg)
        {
            Queue(cancellationTokenSource.Token, new CustomThreadPoolWorkloadWithArguments<T>(cancellationTokenSource, workload, arg));
        }
        public void Queue<T>(Action<T> workload, T arg)
        {
            Queue(default(CancellationTokenSource), workload, arg);
        }

        public void Queue<T1, T2>(CancellationTokenSource cancellationTokenSource, Action<T1, T2> workload, T1 arg1, T2 arg2)
        {
            Queue(cancellationTokenSource, new CustomThreadPoolWorkloadWithArguments<T1, T2>(cancellationTokenSource, workload, arg1, arg2));
        }
        public void Queue<T1, T2>(Action<T1, T2> workload, T1 arg1, T2 arg2)
        {
            Queue(default(CancellationTokenSource), workload, arg1, arg2);
        }

        public void Queue<T1, T2, T3>(CancellationTokenSource cancellationTokenSource, Action<T1, T2, T3> workload, T1 arg1, T2 arg2, T3 arg3)
        {
            Queue(cancellationTokenSource, new CustomThreadPoolWorkloadWithArguments<T1, T2, T3>(cancellationTokenSource, workload, arg1, arg2, arg3));
        }
        public void Queue<T1, T2, T3>(Action<T1, T2, T3> workload, T1 arg1, T2 arg2, T3 arg3)
        {
            Queue(default(CancellationTokenSource), workload, arg1, arg2, arg3);
        }
    }
}