using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Dictionaries;

namespace UtilClasses.Core;

public class FuncUtil<T>
{
    Dictionary<Type, Func<Exception, T>> _handlers = new();

    public FuncUtil<T> Handling<TEx>(Func<TEx, T> f) where TEx : Exception
    {
        _handlers[typeof(TEx)] = e => f((TEx)e);
        return this;
    }


    public Task<T> Function(Func<T> f, [CallerMemberName] string? caller = null) =>
        Function(() => Task.FromResult(f()));
    public async Task<T> Function(Func<Task<T>> f, [CallerMemberName] string? caller = null)
    {
        try
        {
            return await f();
        }
        catch (Exception e)
        {
            var t = e.GetType();
            while (t != null && _handlers.Any())
            {
                var h = _handlers.Maybe(t);
                if (h != null) return h(e);
                t = t.BaseType;
            }
            throw e;
        }
    }
    public async Task<T> Function<TIn>(Func<TIn, Task<T>> f, TIn input, [CallerMemberName] string? caller = null)
        => await Function(() => f(input), caller);
}