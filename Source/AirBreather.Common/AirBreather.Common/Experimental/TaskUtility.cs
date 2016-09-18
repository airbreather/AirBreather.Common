using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather
{
    public static class TaskUtilityExperimental
    {
        private static readonly BlockingCollection<Tuple<long, TaskCompletionSource<bool>>> Targets = new BlockingCollection<Tuple<long, TaskCompletionSource<bool>>>();

        static TaskUtilityExperimental()
        {
            ConcurrentBag<Action<Tuple<long, TaskCompletionSource<bool>>>> spinnerThreads = new ConcurrentBag<Action<Tuple<long, TaskCompletionSource<bool>>>>();
            Thread th = new Thread(() =>
            {
                foreach (var targetAndTcs in Targets.GetConsumingEnumerable())
                {
                    Action<Tuple<long, TaskCompletionSource<bool>>> callback;
                    if (!spinnerThreads.TryTake(out callback))
                    {
                        Tuple<long, TaskCompletionSource<bool>> curr = null;
                        AutoResetEvent evt = new AutoResetEvent(false);
                        Thread child = new Thread(() =>
                        {
                            while (true)
                            {
                                evt.WaitOne();
                                while (Stopwatch.GetTimestamp() < curr.Item1) ;
                                curr.Item2.TrySetResult(true);
                                spinnerThreads.Add(callback);
                            }
                        });

                        child.IsBackground = true;
                        child.Start();

                        callback = twt =>
                        {
                            curr = twt;
                            evt.Set();
                        };
                    }

                    callback(targetAndTcs);
                }
            });
            th.IsBackground = true;
            th.Start();
        }

        public static Task PreciseDelay(long target)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Targets.Add(Tuple.Create(target, tcs));
            return tcs.Task;
        }
    }
}
