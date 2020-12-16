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

            this.WorkloadsFinishedEvents = new ManualResetEvent[concurrency];
            for(var i = 0; i < concurrency; ++i)
            {
                this.WorkloadsFinishedEvents[i] = new ManualResetEvent(true);
            }
            this.WorkloadsReadyEvents = new AutoResetEvent[concurrency];
            for(var i = 0; i < concurrency; ++i)
            {
                this.WorkloadsReadyEvents[i] = new AutoResetEvent(false);
            }
            this.Workloads = new Action[concurrency];

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
                                var workload = Workloads[index];
                                if(workload != null)
                                {
                                    try
                                    {
                                        workload();
                                    }
                                    finally
                                    {
                                        WorkloadsFinishedEvents[index].Set();
                                    }
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

        ManualResetEvent[] WorkloadsFinishedEvents;
        AutoResetEvent[] WorkloadsReadyEvents;
        Action[] Workloads;

        
        public void Dispose()
        {
            // stop threads
            if(ThreadsCancellation != null)
            {
                if(!ThreadsCancellation.IsCancellationRequested)
                {
                    ThreadsCancellation.Cancel();
                    for(var i = 0; i < Threads.Length; ++i)
                    {
                        Threads[i].Join();
                        Threads[i] = null;
                    }
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
                Workloads = null;
            }
        }

        public void Queue(Action workload)
        {
            var idxFreeThread = WaitHandle.WaitAny(WorkloadsFinishedEvents, Timeout.Infinite);
            WorkloadsFinishedEvents[idxFreeThread].Reset();

            Workloads[idxFreeThread] = workload;
            WorkloadsReadyEvents[idxFreeThread].Set();
        }
        public void Queue<T>(Action<T> workload, T arg)
        {
            Queue(() => { workload(arg); });
        }
        public void Queue<T1, T2>(Action<T1, T2> workload, T1 arg1, T2 arg2)
        {
            Queue(() => { workload(arg1, arg2); });
        }
        public void Queue<T1, T2, T3>(Action<T1, T2, T3> workload, T1 arg1, T2 arg2, T3 arg3)
        {
            Queue(() => { workload(arg1, arg2, arg3); });
        }

        // public void WaitPendingAreFinished()
        // {
        //     // wait pending converters and writers are finished
        //     while(true)
        //     {
        //         var nonNullEventsWithIndicies = WorkloadsFinishedEvents.Select((x, i) => new { evt = x, index = i }).Where(x => x.evt != null).ToArray();
        //         if(nonNullEventsWithIndicies.Length > 0)
        //         {
        //             var idxThreadFinished = WaitHandle.WaitAny(nonNullEventsWithIndicies.Select(x => x.evt).ToArray());
        //             WorkloadsFinishedEvents[nonNullEventsWithIndicies[idxThreadFinished].index].Dispose();
        //             WorkloadsFinishedEvents[nonNullEventsWithIndicies[idxThreadFinished].index] = null;
        //         }
        //         else
        //         {
        //             break;
        //         }
        //     }
        // }
    }
}