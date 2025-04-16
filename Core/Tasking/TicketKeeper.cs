using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UtilClasses.Core.Extensions.Semaphores;

namespace UtilClasses.Core.Tasking;

public class TicketKeeper
{
    readonly List<Ticket> _ticketList = new();
    readonly SemaphoreSlim _semaphore = new(1);
    private Ticket DequeueTicket()
    {
        var ret = _ticketList.First();
        _ticketList.RemoveAt(0);
        return ret;
    }
    private void EnqueueTicket(Ticket ticket)
    {
        _ticketList.Add(ticket);
        _ticketList.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
    }

    public virtual Ticket? Get(ulong timestamp)
    {
        if (!_ticketList.Any()) return null;
        return _ticketList.FirstOrDefault()?.Timestamp > timestamp 
            ? null 
            : _semaphore.On(DequeueTicket);
    }

    public async Task<Ticket> SetAsync(Ticket ticket)
    {
        await _semaphore.OnAsync(() => EnqueueTicket(ticket));
        return ticket;
    }
    public async Task<Ticket> SetAsync(ulong ts)
    {
        var ticket = new Ticket(ts);
        await _semaphore.OnAsync(() => EnqueueTicket(ticket));
        return ticket;
    }
    public T Set<T>(T ticket) where T:Ticket
    {
        _semaphore.On(() => EnqueueTicket(ticket));
        return ticket;
    }

    public Ticket Set(ulong ts)
    {
        var ticket = new Ticket(ts);
        _semaphore.On(() => EnqueueTicket(ticket));
        return ticket;
    }

    public bool Peek(out ulong val)
    {
        val = 0;
        if (!_ticketList.Any()) return false;
        val = _ticketList.First().Timestamp;
        return true;
    }
    [ContractAnnotation("=> false, val:null; => true, val:notnull")]
    public bool TryGetBefore(ulong timestamp, out Ticket? val)
    {
        val = null;
        if (!Peek(out var firstTimestamp)) return false;
        if (firstTimestamp > timestamp) return false;
        val = Get(timestamp);
        return true;
    }
    [ContractAnnotation("=> false, val:null; => true, val:notnull")]
    public bool TryGetBefore<T>(ulong timestamp, out Ticket<T>? val)
    {
        val = null;
        var res = TryGetBefore(timestamp, out var x);
        if(!res) return false;

        val = x as Ticket<T>;
        if(null == val) return false;

        return true;
    }

    public bool AnyBefore(ulong timestamp) => Peek(out var first) && first <= timestamp;
}

public class TicketKeeper<TVal> : TicketKeeper
{
    public async Task<Ticket<TVal>> SetAsync (ulong timestamp, TVal val) => (Ticket<TVal>)(await SetAsync(new Ticket<TVal>(timestamp, val)));
    public Ticket<TVal> Set(ulong timestamp, TVal val) => Set(new Ticket<TVal>(timestamp, val));
    public Ticket<TVal> Set(ulong timestamp, TVal val, Action<string> logger)
    {
        logger?.Invoke($"Setting ticket for {timestamp} with value {val}");
        return Set(new Ticket<TVal>(timestamp, val));
    }
}

public record Ticket(ulong Timestamp)
{
    public ulong Timestamp { get; } = Timestamp;
    public void Handled() => Tcs.SetResult(this);
    public TaskCompletionSource<Ticket> Tcs { get; } = new();
    public void Complete() => Tcs.SetResult(this);
}

public record Ticket<TItem>(ulong Timestamp, TItem Item) : Ticket(Timestamp)
{
    public TItem Item { get; } = Item;
}