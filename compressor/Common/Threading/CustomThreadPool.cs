using System;
using System.Collections.Generic;
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
            this.WorkloadsReadyEvents = new ManualResetEventSlim[concurrency];
            for(var i = 0; i < concurrency; ++i)
            {
                this.WorkloadsReadyEvents[i] = new ManualResetEventSlim(false);
            }
            this.Workloads = new Action[concurrency];

            // spin off thread pool
            this.ThreadsCancellation = new CancellationTokenSource();
            this.Threads = new Thread[concurrency];
            for(var i = 0; i < concurrency; ++i)
            {
                Threads[i] = new Thread((obj) => {
                    var index = (int)obj;
                    ThreadPoolThreadId.Value = index;
                    while(true)
                    {
                        try
                        {
                            WorkloadsReadyEvents[index].WaitOne(ThreadsCancellation.Token);
                            WorkloadsReadyEvents[index].Reset();
                            try
                            {
                                try
                                {
                                    if(Workloads[index] != null)
                                    {
                                        var workload = Workloads[index];
                                        if(workload != null)
                                        {
                                            workload();
                                        }
                                    }
                                }
                                finally
                                {
                                    Workloads[index] = null;
                                }
                            }
                            finally
                            {
                                WorkloadsFinishedEvents[index].Release();
                            }
                        }
                        catch(OperationCanceledException)
                        {
                            if(ThreadsCancellation.IsCancellationRequested)
                            {
                                break;
                            }
                            else
                            {
                                throw;
                            }
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
        ManualResetEventSlim[] WorkloadsReadyEvents;
        Action[] Workloads;

        readonly ThreadLocal<int> ThreadPoolThreadId = new ThreadLocal<int>(() => -1);

        public virtual void Dispose()
        {
            // stop threads
            if(ThreadsCancellation != null)
            {
                if(!ThreadsCancellation.IsCancellationRequested)
                {
                    ThreadsCancellation.Cancel();
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

        public void Queue(CancellationToken cancellationToken, Action workload)
        {
            var waitHandles = Enumerable.Concat(WorkloadsFinishedEvents, new [] { cancellationToken.WaitHandle }).ToArray();
            var idxFreeThread = WaitHandle.WaitAny(waitHandles, Timeout.Infinite);
            if(idxFreeThread < WorkloadsFinishedEvents.Length)
            {
                Workloads[idxFreeThread] = workload;
                WorkloadsReadyEvents[idxFreeThread].Set();
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        public void Queue(Action workload)
        {
            Queue(CancellationToken.None, workload);
        }
    }
}