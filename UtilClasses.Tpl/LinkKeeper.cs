using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;
using UtilClasses.Extensions.Dictionaries;

namespace UtilClasses.Dataflow
{

    public class LinkKeeper : IDisposable
    {
        private readonly Dictionary<Guid, List<IDisposable>> _links = new();
        private readonly Dictionary<Guid, List<object>> _stuff = new();
        private Guid ActuallyLink<T>(ISourceBlock<T> src, ITargetBlock<T> trg) => ActuallyLink(src, trg, null, Guid.NewGuid(), new object[] { });
        private Guid ActuallyLink<T>(ISourceBlock<T> src, ITargetBlock<T> trg, Guid id, params object[] stuff) => ActuallyLink(src, trg, null, id, stuff);
        private Guid ActuallyLink<T>(ISourceBlock<T> src, ITargetBlock<T> trg, Guid id, ISource<T> parentSource, ITarget<T> parentTarget) => ActuallyLink(src, trg, null, id, new object[] { parentSource, parentTarget });

        private Guid ActuallyLink<T>(ISourceBlock<T> src, ITargetBlock<T> trg, [CanBeNull] Func<T, bool> predicate,
            Guid id, params object[] stuff) => ActuallyLink(src, trg, predicate, id, (IEnumerable<object>)stuff);
        private Guid ActuallyLink<T>(ISourceBlock<T> src, ITargetBlock<T> trg, [CanBeNull] Func<T, bool> predicate, Guid id, IEnumerable<object> stuff)
        {
            if (Guid.Empty == id)
                id = Guid.NewGuid();

            var link = null == predicate
                ? src.LinkTo(trg, new DataflowLinkOptions() { PropagateCompletion = true })
                : src.LinkTo(trg, new DataflowLinkOptions() { PropagateCompletion = true }, new Predicate<T>(predicate));
            _links.GetOrAdd(id).Add(link);
            _stuff.GetOrAdd(id).AddRange(new object[] { src, trg });
            _stuff.GetOrAdd(id).AddRange(stuff);
            return id;
        }
        public Guid Link<T>(ISource<T> src, ITarget<T> trg, [CanBeNull] Func<T, bool> predicate = null)
            => ActuallyLink(src.OutBlock, trg.InBlock, predicate, Guid.NewGuid(), src, trg);
        public Guid Link<T>(ISource<T> src, ITargetBlock<T> trg, [CanBeNull] Func<T, bool> predicate = null)
            => ActuallyLink(src.OutBlock, trg, predicate, Guid.NewGuid(), src, trg);
        public Guid Link<T>(ISourceBlock<T> src, ITargetBlock<T> trg, [CanBeNull] Func<T, bool> predicate = null)
            => ActuallyLink(src, trg, predicate, Guid.NewGuid());
        public Guid Link<T>(ISourceBlock<T> src, ITarget<T> trg, Func<T, bool> predicate = null) =>
            ActuallyLink(src, trg.InBlock, predicate, Guid.NewGuid(), trg);

        public Guid Transform<T1, T2>(ISourceBlock<T1> src, Func<T1, Task<T2>> transformer, ITargetBlock<T2> trg, int maxDegrees = -1)
        {
            var transformBlock = GetTransformBlock(transformer, maxDegrees);

            var guid = Guid.NewGuid();
            ActuallyLink(src, transformBlock, guid);
            ActuallyLink<T2>(transformBlock, trg, guid);
            return guid;
        }
        public Guid Transform<T1, T2>(ISourceBlock<T1> src, Func<T1, T2> transformer, ITargetBlock<T2> trg, int maxDegrees = -1, Action<Exception> exceptionHandler = null)
        {
            var transformBlock = GetTransformBlock(transformer, maxDegrees, exceptionHandler);

            var guid = Guid.NewGuid();
            ActuallyLink(src, transformBlock, guid);
            ActuallyLink(transformBlock, trg, guid);
            return guid;
        }


        #region Peeking

        public Guid Peek<T>(ISourceBlock<T> src, Action<T> peeker, ITargetBlock<T> trg, int maxDegrees = -1)
            => ActuallyPeek(src, GetTransformBlock(peeker, maxDegrees), trg);
        public Guid Peek<T>(ISourceBlock<T> src, Action<T> peeker, ITarget<T> trg, int maxDegrees = -1)
            => ActuallyPeek(src, GetTransformBlock(peeker, maxDegrees), trg.InBlock, trg);
        public Guid Peek<T>(ISource<T> src, Action<T> peeker, ITarget<T> trg, int maxDegrees = -1)
            => ActuallyPeek(src.OutBlock, GetTransformBlock(peeker, maxDegrees), trg.InBlock, src, trg);
        public Guid Peek<T>(ISource<T> src, Action<T> peeker, ITargetBlock<T> trg, int maxDegrees = -1)
            => ActuallyPeek(src.OutBlock, GetTransformBlock(peeker, maxDegrees), trg, src, trg);

        public Guid Peek<T>(ISourceBlock<T> src, Func<T, Task> peeker, ITargetBlock<T> trg, int maxDegrees = -1)
            => ActuallyPeek(src, GetTransformBlock(peeker, maxDegrees), trg);
        public Guid Peek<T>(ISourceBlock<T> src, Func<T, Task> peeker, ITarget<T> trg, int maxDegrees = -1)
            => ActuallyPeek(src, GetTransformBlock(peeker, maxDegrees), trg.InBlock, trg);
        public Guid Peek<T>(ISource<T> src, Func<T, Task> peeker, ITarget<T> trg, int maxDegrees = -1)
            => ActuallyPeek(src.OutBlock, GetTransformBlock(peeker, maxDegrees), trg.InBlock, src, trg);
        public Guid Peek<T>(ISource<T> src, Func<T, Task> peeker, ITargetBlock<T> trg, int maxDegrees = -1)
            => ActuallyPeek(src.OutBlock, GetTransformBlock(peeker, maxDegrees), trg, src, trg);

        private Guid ActuallyPeek<T>(ISourceBlock<T> src, TransformBlock<T, T> trans, ITargetBlock<T> trg, params object[] stuff)
            => ActuallyPeek(src, trans, trg, Guid.NewGuid(), stuff);


        private Guid ActuallyPeek<T>(ISourceBlock<T> src, TransformBlock<T, T> trans, ITargetBlock<T> trg, Guid id, params object[] stuff)
        {
            if (id == Guid.Empty)
                id = Guid.NewGuid();

            ActuallyLink(src, trans, id);
            ActuallyLink(trans, trg, id);
            _stuff.GetOrAdd(id).Add(new object[] { });
            _stuff.GetOrAdd(id).Add(stuff);
            return id;
        }
        private static ExecutionDataflowBlockOptions GetMaxDegrees(int maxDegrees) =>
            maxDegrees <= 0
                ? new()
                : new() { MaxMessagesPerTask = maxDegrees };
        public static TransformBlock<T1, T2> GetTransformBlock<T1, T2>(Func<T1, T2> transformer, int maxDegrees = -1, Action<Exception> exceptionHandler = null) =>
            new(GetRunner(transformer, exceptionHandler), GetMaxDegrees(maxDegrees));
        public static TransformBlock<T1, T2> GetTransformBlock<T1, T2>(Func<T1, Task<T2>> transformer, int maxDegrees = -1, Action<Exception> exceptionHandler = null) =>
            new(GetRunner(transformer, exceptionHandler), GetMaxDegrees(maxDegrees));

        public static TransformBlock<T, T> GetTransformBlock<T>(Func<T, Task> trans, int maxDegrees, Action<Exception> exceptionHandler = null)
        {
            var runner = GetRunner(trans, exceptionHandler);
            T F(T x)
            {
                runner(x);
                return x;
            }
            return new TransformBlock<T, T>((Func<T, T>) F, GetMaxDegrees(maxDegrees));
        }
        public static TransformBlock<T, T> GetTransformBlock<T>(Action<T> trans, int maxDegrees, Action<Exception> exceptionHandler = null)
        {
            var runner = GetRunner(trans, exceptionHandler);
            T F(T x)
            {
                runner(x);
                return x;
            }
            return new TransformBlock<T, T>((Func<T, T>)F, GetMaxDegrees(maxDegrees));
        }
        public static TransformBlock<T, T> GetTransformBlock<T>(Func<T,T> trans, int maxDegrees, Action<Exception> exceptionHandler = null)
        {
            return new TransformBlock<T, T>(GetRunner(trans, exceptionHandler), GetMaxDegrees(maxDegrees));
        }

        #endregion

        #region LinkToAction
        public Guid LinkToAction<T>(ISourceBlock<T> src, Action<T> action) => ActuallyLink(src, GetActionBlock(action, -1));
        public Guid LinkToAction<T>(ISourceBlock<T> src, Action<T> action, int maxDegrees) => ActuallyLink(src, GetActionBlock(action, maxDegrees));
        public Guid LinkToAction<T>(ISourceBlock<T> src, Func<T, Task> action) => ActuallyLink(src, GetActionBlock(action, -1));
        public Guid LinkToAction<T>(ISourceBlock<T> src, Func<T, Task> action, int maxDegrees) => ActuallyLink(src, GetActionBlock(action, maxDegrees));
        public Guid LinkToAction<T>(ISourceBlock<T> src, Func<T, Task> action, Action<Exception> exceptionHandler) => ActuallyLink(src, GetActionBlock(action, -1, exceptionHandler));
        public Guid LinkToAction<T>(ISource<T> src, Func<T, Task> action) => ActuallyLink(src.OutBlock, GetActionBlock(action, -1), null, Guid.NewGuid(), src);
        public Guid LinkToAction<T>(ISource<T> src, Action<T> action) => ActuallyLink(src.OutBlock, GetActionBlock(action, -1), null, Guid.NewGuid(), src);

        #endregion

        public void Dispose()
        {
            Clear();
            foreach (var s in _stuff.SelectMany(kvp => kvp.Value))
            {
                if (s is not IDisposable disposable)
                    continue;
                disposable.Dispose();
            }
        }

        public void Clear()
        {
            foreach (IDisposable link in _links.Keys.SelectMany(id => _links[id])) 
                link.Dispose();
            
            _links.Clear();
            _stuff.Clear();
        }

        public void Disconnect(Guid id)
        {
            foreach (var link in _links[id])
            {
                link.Dispose();
            }

            _stuff.Remove(id);
        }
        public static ITargetBlock<T> GetActionBlock<T>(Action<T> action, int maxDegrees, Action<Exception> exceptionHandler = null) 
            => new ActionBlock<T>(GetRunner(action, exceptionHandler), GetMaxDegrees(maxDegrees));

        public static ITargetBlock<T> GetActionBlock<T>(Func<T, Task> action, int maxDegrees, Action<Exception> exceptionHandler = null)
            => new ActionBlock<T>(GetRunner(action, exceptionHandler), GetMaxDegrees(maxDegrees));

        private static Func<T, Task> GetRunner<T>(Func<T, Task> func, Action<Exception> exceptionHandler)
        {
            if (null == exceptionHandler)
                return func;
            return async x =>
            {
                try
                {
                    await func(x);
                }
                catch (Exception e)
                {
                    exceptionHandler?.Invoke(e);
                }
            };
        }
        private static Func<T, T2> GetRunner<T, T2>(Func<T, T2> func, Action<Exception> exceptionHandler)
        {
            if (null == exceptionHandler)
                return func;
            return x =>
            {
                try
                {
                    return func(x);
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                }

                return default;
            };
        }

        private static Func<T, Task<T2>> GetRunner<T, T2>(Func<T, Task<T2>> func, Action<Exception> exceptionHandler)
        {
            if (null == exceptionHandler)
                return func;
            return async x =>
            {
                try
                {
                    return await func(x);
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                }

                return default;
            };
        }

        private static Action<T> GetRunner<T>(Action<T> action, [CanBeNull] Action<Exception> exceptionHandler)
        {
            if (null == exceptionHandler)
                return action;
            return x =>
            {
                try
                {
                    action(x);
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                }
            };
        }
    }
}
