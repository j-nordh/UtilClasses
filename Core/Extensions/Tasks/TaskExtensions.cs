using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Enumerables;

namespace UtilClasses.Core.Extensions.Tasks;

public static class TaskExtensions
{
    public static bool IsRunning(this Task t)
    {
        return !(t.IsCanceled || t.IsCompleted || t.IsFaulted );
    }
    public static void Forget(this Task task)
    {
        task.ConfigureAwait(false);
    }

    public static async Task<T> ThrowOnTimeout<T>(this Task<T> t, int ms) =>
        await t.WithTimeout(ms, () => throw new TimeoutException());
    public static async Task<T> WithTimeout<T>(this Task<T> t, int ms, Func<T> onTimeout)
    {
        var first = await new[] {t, Task.Delay(ms).ContinueWith(_ => default(T))}.WhenAny();
        return first.Equals(t) ? t.Result : onTimeout();
    }

    public static async Task MaybeInvoke(this Func<Task>? f)
    {
        if (f == null)
            await Task.CompletedTask;
        else
            await f();
    }
    public static async Task MaybeInvoke<T>(this Func<T, Task>? f, T arg )
    {
        if (f == null)
            await Task.CompletedTask;
        else
            await f(arg);
    }
    public static async Task MaybeInvoke<T1, T2>(this Func<T1, T2, Task>? f, T1 arg1, T2 arg2)
    {
        if (f == null)
            await Task.CompletedTask;
        else
            await f(arg1, arg2);
    }
    ///// <summary>
    ///// Returns a task that either ends with the completion of the provided task or the end of the specified timeout.
    ///// </summary>
    ///// <param name="t">The task that is expected to complete within the specified timespan</param>
    ///// <param name="ms">The timespan (in milliseconds) allowed for the task to complete</param>
    ///// <returns>True if the task finished before the timeout, false otherwise.</returns>
    //public static async Task<bool> WithTimeout(this Task t, int ms) => await t.WithTimeout(TimeSpan.FromMilliseconds(ms));
    //public static async Task<bool> WithTimeout(this Task t, TimeSpan timeout)
    //{
    //    var first = await new[] {t, Task.Delay(timeout)}.WhenAny();
    //    if (first == t)
    //        return true;

    //    return first.Equals(t);
    //}
    //public static async Task<(bool Completed, T Result)> WithTimeout<T>(this Task<T> t, int ms) => await t.WithTimeout(TimeSpan.FromMilliseconds(ms));
    //public static async Task<(bool Completed, T Result)> WithTimeout<T>(this Task<T> t, TimeSpan timeout) 
    //{
    //    var first = await new[] { t, Task.Delay(timeout) }.WhenAny();
    //    return first.Equals(t) ? (true, t.Result) : (false, default);
    //}

    public static Task<T> AsTask<T>(this T obj) => Task.FromResult(obj);
    public static Func<Task> AsTask(this Action a) => ()=>
    {
        a();
        return Task.CompletedTask;
    };
    public static Func<T,Task> AsTask<T>(this Action<T> a) => x =>
    {
        a(x);
        return Task.CompletedTask;
    };

    public static Func<T1, T2, Task> AsTask<T1, T2>(this Action<T1, T2> a) => (arg1, arg2) =>
    {
        a(arg1, arg2);
        return Task.CompletedTask;
    };
    public static Func<T1, T2, T3,Task> AsTask<T1, T2,  T3>(this Action<T1, T2, T3> a) => (arg1, arg2, arg3) =>
    {
        a(arg1, arg2, arg3);
        return Task.CompletedTask;
    };
    public static Func<T1, Task> AsActionTask<T1, T2>(this Func<T1, T2> a) => arg1 =>
    {
        a(arg1);
        return Task.CompletedTask;
    };
        
    public static Func<T1, T2,Task<T3>> AsTask<T1, T2, T3>(this Func<T1, T2, T3> f) => (arg1, arg2) => Task.FromResult( f(arg1, arg2));
    public static Func<T1, T2, Task> AsActionTask<T1, T2, T3>(this Func<T1, T2, T3> f) => (arg1, arg2) =>
    {
        f(arg1, arg2);
        return Task.CompletedTask;
    };
    public static Task RunAsTask(this Action a) => Task.Run(a);
    public static Task<T> RunAsTask<T>(this Func<T> f) => Task.Run(f);
    public static Task<T?> SuccessOrNull<T>(this Task<T> task) where T : class =>
        task.ContinueWith(t => (t.IsFaulted || t.IsCanceled) ? null : t.Result);

    public static Task<T?> SuccessOrNull<T>(this Task<T?> task) where T : struct =>
        task.ContinueWith(t => (t.IsFaulted || t.IsCanceled) ? null : t.Result);

    public static Task OnFault(this Task task, Action<Exception> handler) => task.ContinueWith(t =>
    {
        if (!t.IsFaulted) return;
        handler(t.Exception);
    });

    public static Task<T> OnFault<T>(this Task<T> task, Func<Exception, T> handler) =>
        task.ContinueWith(t => t.IsFaulted ? handler(t.Exception) : t.Result);


    public static async Task<T> OnSuccess<T>(this Task<T> task, Func<T, Task<T>> f)
    {
        var t2 = await task.ContinueWith(t =>
        {
            if (t.IsFaulted)
                throw t.Exception;
            if (t.IsCanceled)
                throw new TaskCanceledException("The task was canceled");
            return t.Result;
        });
        return await f(t2);
    }
    public static async Task<T> OnSuccess<T>(this Task<T> task, Func<T, T> f)
    {
        var t2 = await task.ContinueWith(t =>
        {
            if (t.IsFaulted)
                throw t.Exception;
            if (t.IsCanceled)
                throw new TaskCanceledException("The task was canceled");
            return t.Result;
        });
        return f(t2);
    }
    public static async Task<T> OnSuccess<T>(this IEnumerable<Task> tasks, Func<Task<T>> f)
    {
        var tasks2 = tasks.ForEach(task => task.ContinueWith(t =>
        {
            if (t.IsFaulted)
                throw t.Exception;
            if (t.IsCanceled)
                throw new TaskCanceledException("The task was canceled");
            return t;
        }));
        await tasks2.WhenAll();
        return await f();
    }
    public static async Task<T> OnSuccess<T>(this Task task, Func<Task<T>> f)
    {
        await task.ContinueWith(t =>
        {
            if (t.IsFaulted)
                throw t.Exception;
            if (t.IsCanceled)
                throw new TaskCanceledException("The task was canceled");
            return t;
        });
        return await f();
    }
    public static async Task<T> OnSuccess<T>(this IEnumerable<Task> tasks, T o)
    {
        var tasks2 = tasks.ForEach(task => task.ContinueWith(t =>
        {
            if (t.IsFaulted)
                throw t.Exception;
            if (t.IsCanceled)
                throw new TaskCanceledException("The task was canceled");
            return t;
        }));
        await tasks2.WhenAll();
        return o;
    }

    public static async Task<TOut> Try<TIn, TOut>(this Task<TIn> t, TOut onSuccess, TOut onFail)
    {
        try
        {
            await (t);
            return onSuccess;
        }
        catch (Exception)
        {
            return onFail;
        }
    }


    public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
    public static async Task WhenAll_Throw(this IEnumerable<Task> tasks)
    {
        var lst = tasks.ToList();
        await Task.WhenAll(lst);
        var exceptions = new List<Exception>();
        foreach (var task in lst.Where(t=>t.IsFaulted))
        {
            exceptions.Add(task.Exception);
        }

        if (!exceptions.Any()) return;
        if (exceptions.Count == 1) throw exceptions.Single();
        throw new AggregateException(exceptions);
    }
    public static async Task<T[]> WhenAll_Throw<T>(this IEnumerable<Task<T>> tasks)
    {
        var lst = tasks.ToList();
        var ret = await Task.WhenAll(lst);
        var exceptions = new List<Exception>();
        foreach (var task in lst.Where(t => t.IsFaulted))
        {
            exceptions.Add(task.Exception);
        }

        if (!exceptions.Any()) return ret;
        if (exceptions.Count == 1) throw exceptions.Single();
        throw new AggregateException(exceptions);
    }

    public static Task<Task> WhenAny(this IEnumerable<Task> tasks) => Task.WhenAny(tasks);
    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) => Task.WhenAll(tasks);

    public static async Task<List<TOut>> SelectManyAsync<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, List<TOut>> f)
    {
        var lst = new List<TOut>();
        await items.RunAsync(i =>
        {
            var ret = f(i);
            lock (lst)
                lst.AddRange(ret);
        }).WhenAll();
        return lst;
    }
    public static IEnumerable<Task> RunAsync<T>(this IEnumerable<T> items, Action<T> a) =>
        items.Select(i => Task.Run(() => a(i)));



    #region LINQ
    public static async Task<List<TOut>> Select<TIn, TOut>(this IEnumerable<Task<TIn>> items, Func<TIn, TOut> f)
    {
        var lst = new List<TOut>();
        foreach (var i in items)
        {
            lst.Add(f(await i));
        }
        return lst;
    }


    public static async Task<IEnumerable<TOut>> Select<TIn, TOut>(this Task<IEnumerable<TIn>> items, Func<TIn, TOut> f)
    {
        return (await items).Select(f);
    }
    public static async Task<IEnumerable<TOut>> SelectMany<TIn, TOut>(this Task<IEnumerable<TIn>> items, Func<TIn, IEnumerable<TOut>> f)
    {
        var ret = new List<TOut>();
        foreach (var i in await (items))
        {
            ret.AddRange(f(i));
        }
        return ret;
    }


    public static async Task<IEnumerable<TOut>> SelectMany<TIn, TOut>(this IEnumerable<Task<TIn>> items, Func<TIn, IEnumerable<TOut>> f)
    {
        var ret = new List<TOut>();
        foreach (var i in items)
        {
            ret.AddRange(f(await i));
        }
        return ret;
    }
    public static async Task<IEnumerable<T>> SelectMany<T>(this Task<IEnumerable<IEnumerable<T>>> items)
    {
        var ret = new List<T>();
        foreach (var i in await (items))
        {
            ret.AddRange(i);
        }
        return ret;
    }
    public static async Task<IEnumerable<T>> SelectMany<T>(this Task<List<T>[]> items)
    {
        var ret = new List<T>();
        foreach (var i in await (items))
        {
            ret.AddRange(i);
        }
        return ret;
    }

    public static async Task<IEnumerable<TOut>> Cast<TIn, TOut>(this Task<IEnumerable<TIn>> os) =>
        (await os).Cast<TOut>(); 
    #endregion
    public static T RunAndGet<T>(this Task<T> t) where T : class
    {
        var tcs = new TaskCompletionSource<T>();
        Task.Run(async () => tcs.SetResult(await t)).ContinueWith(task => { if (task.IsFaulted) tcs.SetException(task.Exception); });
        return tcs.Task.Result;
    }
        
}