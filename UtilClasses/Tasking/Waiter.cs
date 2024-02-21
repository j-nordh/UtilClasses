using System;
using System.Collections.Generic;
using System.Threading;
using UtilClasses.Interfaces;


namespace UtilClasses.Tasking;

public class Waiter
{
    Dictionary<ulong, (HashSet<Guid> Set, SemaphoreSlim Semaphore)> _waiters = new();
    private readonly int _producerCount;

    public Waiter(int producerCount)
    {
        _producerCount = producerCount;
    }
    public void AddMarker(ulong timestamp)
    {
        if (_waiters.ContainsKey(timestamp))
            throw new Exception($"Cannot register a new WaitPoint for {timestamp}");
        _waiters[timestamp] = (new HashSet<Guid>(), new SemaphoreSlim(0));
    }

    public void Register(ulong timestamp, IHasGuid obj) => Register(timestamp, obj.Id);
    public void Register(ulong timestamp, Guid id)
    {
        if(!_waiters.TryGetValue(timestamp, out var tuple))
            return;
        if(tuple.Set.Contains(id))
            Console.WriteLine($"Tried to register a production for {id} twice");
        tuple.Set.Add(id);
        if (tuple.Set.Count < _producerCount)
            return;

        tuple.Semaphore.Release();
    }

    public void Remove(ulong timestamp)
    {
        if (!_waiters.ContainsKey(timestamp)) return;
        var (_, semaphore) = _waiters[timestamp];
        semaphore.Release();
        _waiters.Remove(timestamp);
    }

    public void Wait(ulong timestamp)
    {
        if (!_waiters.TryGetValue(timestamp, out var tuple))
            throw new Exception("Tried to wait for an unregistered timestamp");
        tuple.Semaphore.Wait();
        _waiters.Remove(timestamp);
    }
}