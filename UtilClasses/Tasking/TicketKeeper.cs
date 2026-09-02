using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Extensions.Semaphores;

namespace UtilClasses.Tasking;

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
        SortTickets();
    }

    private void SortTickets() => _ticketList.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

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

    /// <summary>
    /// Moves an enqueued ticket to a new timestamp, keeping its <see cref="Ticket.Tcs"/> so that
    /// whoever is awaiting the ticket is still completed when it eventually fires. Returns false
    /// if the ticket is no longer enqueued, i.e. <see cref="Get"/> has already handed it out.
    /// <para>
    /// Moving a ticket into the past makes it fire on the next timestamp it is checked against.
    /// The queue is ordered by timestamp, so moving a ticket ahead of another one reorders them:
    /// it is the caller's business not to move a ticket past one that has to be handled first.
    /// </para>
    /// </summary>
    public bool Reschedule(Ticket ticket, ulong timestamp) => _semaphore.On(() =>
    {
        //By identity: Ticket is a record, so its generated equality compares members rather than
        //instances. That happens to tell two same-valued tickets apart today, since each carries
        //its own Tcs, but identity is what is actually meant here.
        var idx = _ticketList.FindIndex(t => ReferenceEquals(t, ticket));
        if (idx < 0) return false;
        _ticketList[idx].MoveTo(timestamp);
        SortTickets();
        return true;
    });
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
    //Settable so that TicketKeeper.Reschedule can move a ticket without replacing it, which would
    //hand the waiter a Tcs that is never completed. Note that this makes the record's generated
    //equality and hash code mutable too, so tickets must not be used as dictionary keys.
    public ulong Timestamp { get; private set; } = Timestamp;
    public void Handled() => Tcs.SetResult(this);
    public TaskCompletionSource<Ticket> Tcs { get; } = new();
    public void Complete() => Tcs.SetResult(this);

    /// <summary>
    /// Only for <see cref="TicketKeeper.Reschedule"/>: the keeper's queue is kept sorted by
    /// timestamp, so a ticket that moves while enqueued has to be re-sorted by the keeper.
    /// </summary>
    internal void MoveTo(ulong timestamp) => Timestamp = timestamp;
}

public record Ticket<TItem>(ulong Timestamp, TItem Item) : Ticket(Timestamp)
{
    public TItem Item { get; } = Item;
}