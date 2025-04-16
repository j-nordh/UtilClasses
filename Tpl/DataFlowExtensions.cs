using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using UtilClasses.Dataflow;

namespace UtilClasses.DataFlow
{
    public static class DataFlowExtensions
    {
        public static ISourceBlock<TOut> To<TIn, TOut>(this ISourceBlock<TIn> src, IPropagatorBlock<TIn, TOut> part, out IDisposable link, bool discardNull = true)
        {
            link = discardNull ? src.LinkTo(part, x => x != null) : src.LinkTo(part);
            return part;
        }
        public static ISourceBlock<TOut> To<TIn, TOut>(this ISourceBlock<TIn> src, Func<TIn, TOut> f, bool discardNull = true) => src.To(new TransformBlock<TIn, TOut>(f), out _, discardNull);
        public static ISourceBlock<TOut> To<TIn, TOut>(this ISourceBlock<TIn> src, Func<TIn, TOut> f, out IDisposable link, bool discardNull = true) => src.To(new TransformBlock<TIn, TOut>(f), out link, discardNull);
        public static ISourceBlock<T> To<T>(this ISourceBlock<T> src, Action<T> a, bool discardNull = true) => src.To(new TransformBlock<T, T>(x => { a(x); return x; }), out var _, discardNull);
        public static ISourceBlock<T> To<T>(this ISourceBlock<T> src, Action<T> a, out IDisposable link, bool discardNull = true) => src.To(new TransformBlock<T, T>(x => { a(x); return x; }), out link, discardNull);
        public static ISourceBlock<T> Split<T>(this ISourceBlock<IEnumerable<T>> src, out IDisposable link, bool discardNull = true) => src.To(new TransformManyBlock<IEnumerable<T>, T>(x => x), out link, discardNull);

        public static IDisposable Sink<T>(this ISourceBlock<T> src, ITargetBlock<T> target, bool discardNull = true) =>
            discardNull
                ? src.LinkTo(target, x => x != null)
                : src.LinkTo(target);

        public static void Sink<T>(this ISourceBlock<T> src, Action<T> a, bool discardNull = true) =>
            src.Sink(new ActionBlock<T>(a), discardNull);

        public static ISourceBlock<T[]> Batch<T>(this ISourceBlock<T> src, int batchSize, bool discardNull = true) => src.To(new BatchBlock<T>(batchSize), out _, discardNull);
        public static ISourceBlock<T[]> Batch<T>(this ISourceBlock<T> src, int batchSize, out IDisposable link, bool discardNull = true) => src.To(new BatchBlock<T>(batchSize), out link, discardNull);
        public static IDisposable LinkTo<T>(this ISourceBlock<T> src, ITarget<T> trg) => src.LinkTo(trg.InBlock);

        public static IDisposable LinkToAction<T>(this ISourceBlock<T> src, Action<T> a, ExecutionDataflowBlockOptions options) => src.LinkTo(new ActionBlock<T>(a, options));
        public static IDisposable LinkToAction<T>(this ISourceBlock<T> src, Action<T> a) => src.LinkTo(new ActionBlock<T>(a));
        public static IDisposable LinkToAction<T>(this ISourceBlock<T> src, Func<T, Task> f) => src.LinkTo(new ActionBlock<T>(f));
        public static IDisposable LinkToAction<T>(this ISourceBlock<T> src, Func<T, Task> f, Action<Exception> e) => src.LinkTo(new ActionBlock<T>(async x =>
        {
            try
            {
                await f(x);
            }
            catch (Exception exception)
            {
                e(exception);
            }
        }));
        public static IDisposable LinkToAction<T>(this ISourceBlock<T> src, Func<T, Task> f, ExecutionDataflowBlockOptions options) => src.LinkTo(new ActionBlock<T>(f, options));
        public static IDisposable LinkToAction<T>(this ISourceBlock<T> src, Func<T, Task> f, int maxDegreeOfParallelism) =>
            src.LinkTo(new ActionBlock<T>(f, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism }));

        public static async Task Teardown<T>(this ITargetBlock<T> b)
        {
            if (null == b)
                return;
            b.Complete();
            await b.Completion;
        }

        public static async Task<bool> WhenEmpty<T>(this BufferBlock<T> block, CancellationToken t)
        {
            while (!t.IsCancellationRequested)
            {
                if (block.Count == 0) return true;
                await Task.Delay(5, t);
            }

            return false;
        }

        public static async Task WhenEmpty<T>(this BufferBlock<T> block, int timeoutMs) =>
            await block.WhenEmpty(new CancellationTokenSource(timeoutMs).Token);


        public static async Task<bool> WhenEmpty<T1, T2>(this TransformBlock<T1, T2> block, CancellationToken t)
        {
            do
            {
                if (block.InputCount != block.OutputCount) return true;
                await Task.Delay(5, t);
            } while (!t.IsCancellationRequested);

            return false;

        }
        public static async Task WhenEmpty<T1, T2>(this TransformBlock<T1, T2> block, int timeoutMs) =>
            await block.WhenEmpty(new CancellationTokenSource(timeoutMs).Token);

        public static void OnException<T>(this ISourceBlock<T> block, Action<Exception> a)
        {
            block.Completion.ContinueWith(t =>
            {
                if (t.IsCompleted)
                    return;
                if (t.IsFaulted)
                    Console.WriteLine(t.Exception);
            });
        }
        public static void OnException<T>(this ISource<T> block, Action<Exception> a)
        {
            block.OutBlock.Completion.ContinueWith(t =>
            {
                if (t.IsCompleted)
                    return;
                if (t.IsFaulted)
                    Console.WriteLine(t.Exception);
            });
        }

    }
}