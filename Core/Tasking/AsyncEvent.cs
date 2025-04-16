using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Tasks;

namespace UtilClasses.Core.Tasking;

public abstract class AsyncEventBase<T>
{
    //protected readonly bool _parallel;
    //private readonly bool _background;
    protected readonly Dictionary<Guid, T> _funcs = new();

    public Guid Subscribe(T f)
    {
        var id = Guid.NewGuid();
        _funcs.Add(id, f);
        return id;
    }

    public bool Unsubscribe(Guid id) => _funcs.Remove(id);
}
public class AsyncEvent:AsyncEventBase<Func<Task>>
{
    public Guid Subscribe(Action a) => Subscribe(a.AsTask());
    public async Task Invoke()
    {
        foreach (var f in _funcs.Values)
        {
            await f();
        }
    }
}

public class AsyncEvent<T>: AsyncEventBase<Func<T,Task>>
{

    public Guid Subscribe(Action<T> a) => Subscribe(a.AsTask());

    public async Task Invoke(T arg)
    {
        foreach (var f in _funcs.Values)
        {
            await f(arg);
        }
    }
}
public class AsyncEvent<T1,T2> : AsyncEventBase<Func<T1, T2, Task>>
{

    public Guid Subscribe(Action<T1, T2> a) => Subscribe(a.AsTask());

    public async Task Invoke(T1 arg1, T2 arg2)
    {
        foreach (var f in _funcs.Values)
        {
            await f(arg1, arg2);
        }
    }
}