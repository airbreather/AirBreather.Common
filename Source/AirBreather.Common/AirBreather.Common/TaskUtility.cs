﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather
{
    public static class TaskUtility
    {
        public static IEnumerable<Task<T>> InCompletionOrder<T>(this IEnumerable<Task<T>> tasks)
        {
            // Stephen Toub named his version of this method "Interleaved" in the TAP guide.
            // I picked Jon Skeet's name for this method because it's more expressive.
            // ...
            // Rx.NET has IObservable<Task<T>> --> IObservable<T> called "Merge".
            // It does basically the same thing as this, except it takes advantage
            // of the fact that IObservable<T> supports non-success messages natively,
            // so it can return IObservable<T> instead of keeping the Task<T> wrapper.
            tasks.ValidateNotNull(nameof(tasks));
            List<Task<T>> taskList = tasks.ToList();

            TaskCompletionSource<T>[] outputSources = new TaskCompletionSource<T>[taskList.Count];
            for (int i = 0; i < outputSources.Length; i++)
            {
                outputSources[i] = new TaskCompletionSource<T>();
            }

            int highestCompletedIndex = -1;
            foreach (Task<T> task in taskList)
            {
                task.ContinueWith(t =>
                {
                    TaskCompletionSource<T> outputSource = outputSources[Interlocked.Increment(ref highestCompletedIndex)];
                    switch (t.Status)
                    {
                        case TaskStatus.Canceled:
                            outputSource.SetCanceled();
                            break;

                        case TaskStatus.Faulted:
                            outputSource.SetException(t.Exception.InnerExceptions);
                            break;

                        ////case TaskStatus.RanToCompletion:
                        default:
                            outputSource.SetResult(t.Result);
                            break;
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }

            return outputSources.Select(outputSource => outputSource.Task);
        }

        // https://msdn.microsoft.com/en-us/library/hh873178.aspx#Anchor_2
        public static Task WaitOneAsync(this WaitHandle waitHandle)
        {
            waitHandle.ValidateNotNull(nameof(waitHandle));
            var tcs = new TaskCompletionSource<bool>();
            var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, (_, __) => tcs.SetResult(true), null, -1, true);
            var t = tcs.Task;
            t.ContinueWith(_ => rwh.Unregister(null));
            return t;
        }
    }
}