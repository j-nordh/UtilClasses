using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Tasks;

namespace UtilClasses.Core.Tasking;

public interface IAsyncEvent
{
    
    Guid SubscribeGuid(Func<CancellationToken, Task> f);
    IDisposable SubscribeTicket(Func<CancellationToken, Task> f);
    bool Unsubscribe(Guid id);
}
public interface IAsyncEvent<out T>
{
    Guid SubscribeGuid(Func<T, CancellationToken, Task> f);
    IDisposable SubscribeTicket(Func<T, CancellationToken, Task> f);
    bool Unsubscribe(Guid id);
}

public static class AsyncEventExtensions
{
    public static Guid SubscribeGuid(this IAsyncEvent e, Action<CancellationToken> a) => e.SubscribeGuid(a.AsTask());
    public static Guid SubscribeGuid(this IAsyncEvent e, Action a) => e.SubscribeGuid(a.AsTaskWithCt());
    public static IDisposable SubscribeTicket(this IAsyncEvent e, Action<CancellationToken> a) => e.SubscribeTicket(a.AsTask());
    public static IDisposable SubscribeTicket(this IAsyncEvent e, Action a) => e.SubscribeTicket(a.AsTaskWithCt());

    public static Guid SubscribeGuid<T>(this IAsyncEvent<T> e, Action<T, CancellationToken> a) => e.SubscribeGuid(a.AsTask());
    public static Guid SubscribeGuid<T>(this IAsyncEvent<T> e, Action<T> a) => e.SubscribeGuid(a.AsTaskWithCt());
    public static IDisposable SubscribeTicket<T>(this IAsyncEvent<T> e, Action<T,CancellationToken> a) => e.SubscribeTicket(a.AsTask());
    public static IDisposable SubscribeTicket<T>(this IAsyncEvent<T> e, Action<T> a) => e.SubscribeTicket(a.AsTaskWithCt());
}

public abstract class AsyncEventBase<T>
{
    //protected readonly bool _parallel;
    //private readonly bool _background;
    protected readonly Dictionary<Guid, T> _funcs = new();

    public Guid SubscribeGuid(T f)
    {
        var id = Guid.NewGuid();
        _funcs.Add(id, f);
        return id;
    }

    public IDisposable SubscribeTicket(T f)
    {
        var id = SubscribeGuid(f);
        return new Ticket(id, this);
    }

    private class Ticket :IDisposable
    {
        private readonly Guid _id;
        private readonly AsyncEventBase<T> _aeb;

        public Ticket(Guid id, AsyncEventBase<T> aeb)
        {
            _id = id;
            _aeb = aeb;
        }
        public void Dispose()
        {
            _aeb.Unsubscribe(_id);
        }
    }

    public bool Unsubscribe(Guid id) => _funcs.Remove(id);
}
public class AsyncEvent:AsyncEventBase<Func<CancellationToken, Task>>,IAsyncEvent
{
    public Guid SubscribeGuid(Action<CancellationToken> f) => SubscribeGuid(f.AsTask());
    public async Task Invoke(CancellationToken ct = default)
    {
        foreach (var f in _funcs.Values)
        {
            await f(ct);
        }
    }
}

public class AsyncEvent<T>: AsyncEventBase<Func<T,CancellationToken, Task>>, IAsyncEvent<T>
{
    public Guid SubscribeGuid(Action<T, CancellationToken> a) => SubscribeGuid(a.AsTask());
    
    public async Task Invoke(T arg, CancellationToken ct = default)
    {
        foreach (var f in _funcs.Values)
        {
            await f(arg, ct);
        }
    }

    
}
public class AsyncEvent<T1,T2> : AsyncEventBase<Func<T1, T2, Task>>
{

    public Guid SubscribeGuid(Action<T1, T2> a) => SubscribeGuid(a.AsTask());

    public async Task Invoke(T1 arg1, T2 arg2)
    {
        foreach (var f in _funcs.Values)
        {
            await f(arg1, arg2);
        }
    }
}