using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Dictionaries;
using UtilClasses.Core.Extensions.Tasks;

namespace UtilClasses.Core;

public class CallbackRegistry <TKey, TArg>
{
    private Dictionary<Guid, Action<TArg>> _cbs;
    private Dictionary<TKey, Queue<Guid>> _queues;
    private Dictionary<Guid, CancellationTokenSource> _ctss;

    public CallbackRegistry()
    {
        _cbs = new Dictionary<Guid, Action<TArg>>();
        _queues = new Dictionary<TKey, Queue<Guid>>();
        _ctss = new Dictionary<Guid, CancellationTokenSource>();
    }

    public Guid Register(TKey key, Action<TArg> arg)
    {
        var id = Guid.NewGuid();
        lock(_cbs)
        {
            _cbs[id] = arg;
            _queues.GetOrAdd(key).Enqueue(id);
        }
        return id;
    }
    public Guid Register(TKey key, TaskCompletionSource<TArg> tcs) => Register(key, r => tcs.TrySetResult(r));
    public Guid Register(TKey key, TaskCompletionSource<TArg> tcs, int timeOut)
    {
        var cbId = Register(key, r => tcs.TrySetResult(r));
        var cts = new CancellationTokenSource(timeOut);
        cts.Token.Register(() =>
        {
            if (tcs.Task.IsRunning()) tcs.SetCanceled();
            UnRegister(cbId);
            _ctss.Remove(cbId);
            cts.Dispose();
        });
        return cbId;

    }

    public void UnRegister(Guid id)
    {
        lock (_cbs)
        {
            _cbs.Remove(id);
        }
            
    }

    public bool Trigger(TKey key, TArg arg)
    {
        lock (_cbs)
        {
            var q = _queues.Maybe(key);
            if (null == q) return false;
            while (q.Any())
            {
                var id = q.Dequeue();
                var a = _cbs.Maybe(id);
                if (null == a) continue;
                a(arg);
                return true;
            }
            return false;
        }
    }
}