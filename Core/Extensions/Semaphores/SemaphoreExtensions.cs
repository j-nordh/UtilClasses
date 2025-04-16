using System;
using System.Threading;
using System.Threading.Tasks;

namespace UtilClasses.Core.Extensions.Semaphores;

public static class SemaphoreExtensions
{
    public static async Task OnAsync(this SemaphoreSlim sem, Action a)
    {
        var ct = new CancellationToken();
        await sem.ActualOnAsync(ct, a, null);
    }
    public static async Task OnAsync(this SemaphoreSlim sem, Action a, Action<Exception> exceptionHandler)
    {
        var cts = new CancellationTokenSource();
        await sem.ActualOnAsync(cts.Token, a, exceptionHandler);
    }
    public static async Task OnAsync(this SemaphoreSlim sem, Func<Task> f, Action<Exception>? exceptionHandler = null)
    {
        var ct = new CancellationToken();
        await sem.ActualOnAsync(ct, f, exceptionHandler);
    }

    private class Ticket : IDisposable
    {
        private readonly SemaphoreSlim _sem;

        public Ticket(SemaphoreSlim sem)
        {
            _sem = sem;
        }

        public void Dispose()
        {
            _sem.Release();
        }
    }
    public static async Task<IDisposable> TicketAsync(this SemaphoreSlim sem)
    {
        await sem.WaitAsync();
        return new Ticket(sem);
    }
    public static async Task<T> OnAsync<T>(this SemaphoreSlim sem, Func<Task<T>> f)
    {
        var cts = new CancellationTokenSource();
        return await sem.ActualOnAsync(cts.Token, f, null);
    }
    public static async Task OnAsync<T>(this SemaphoreSlim sem, Func<T, Task> f, T input)
    {
        var ct = new CancellationToken();
        await sem.ActualOnAsync(ct, ()=>f(input), null);
    }
    public static async Task<T> OnAsync<T>(this SemaphoreSlim sem, Func<T> f, int timeoutMs, Action<Exception>? exceptionHandler = null)
    {
        var cts = new CancellationTokenSource(timeoutMs);
        return await sem.ActualOnAsync(cts.Token, f, exceptionHandler);
    }
    public static async Task<T> OnAsync<T>(this SemaphoreSlim sem, Func<T> f, Action<Exception>? exceptionHandler = null)
    {
        var ct = new CancellationToken();
        return await sem.ActualOnAsync(ct, f, exceptionHandler);
    }


    public static async Task<T> OnAsync<T>(this SemaphoreSlim sem, Func<T> f, CancellationToken ct, Action<Exception>? exceptionHandler = null)
    {
        return await sem.ActualOnAsync(ct, f, null);
    }

    public static async Task OnAsync(this SemaphoreSlim sem, CancellationToken ct, Action a)
    {
        await sem.WaitAsync(ct);
        try
        {
            a();
        }
        finally
        {
            sem.Release();
        }
    }

    private static async Task ActualOnAsync(this SemaphoreSlim sem, CancellationToken ct, Action a, Action<Exception>? exceptionHandler)
    {
        Task<int> Wrap()
        {
            a();
            return Task.FromResult(0);
        }
        await sem.ActualOnAsync(ct, Wrap, exceptionHandler);
    }

    private static async Task<T> ActualOnAsync<T>(this SemaphoreSlim sem, CancellationToken ct, Func<T> f, Action<Exception>? exceptionHandler)
    {
        Task<T> Wrap() => Task.FromResult(f());
        return await sem.ActualOnAsync(ct, Wrap, exceptionHandler);
    }
    private static async Task<T> ActualOnAsync<T>(this SemaphoreSlim sem, CancellationToken ct, Func<Task<T>> f, Action<Exception>? exceptionHandler)
    {   
        try
        {
            await sem.WaitAsync(ct);
            return await f();
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
        }
        finally
        {
            sem.Release();
        }

        return default;
    }
    public static void On(this SemaphoreSlim sem, Action a, Action<Exception>? exceptionHandler=null)
    {

        try
        {
            sem.Wait();
            a();
        }
        catch (Exception e)
        {
            exceptionHandler?.Invoke(e);
        }
        finally
        {
            sem.Release();
        }
    }
    public static T On<T>(this SemaphoreSlim sem, Func<T> f)
    {
        sem.Wait();
        try
        {
            return f();
        }
        finally
        {
            sem.Release();
        }
    }
}