using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UtilClasses.Core.Cli;
using UtilClasses.Core.Extensions.Dictionaries;

namespace UtilClasses.Core;

public static class CTrace
{
    private static ConsoleWriter _wr = new();
    public static bool Enabled { get; set; }
    private static Dictionary<Guid, int> _counters = new ();
    private static Stack<Guid> _callStack = new();
    private static Dictionary<Guid, (DateTime Dt, string Name)> _startTimes = new ();
        
    public static void Go(string s)
    {
        if (!Enabled) return;
        _wr.WriteLine(s);
    }

    public static Guid Enter([CallerMemberName] string? s = null)
    {
        Go($"Entering {s}:");
        _wr.Indent = _wr.Indent + "  ";
        var id = Guid.NewGuid();
        _callStack.Push(id);
        return id;
    }
    public static void Exit([CallerMemberName] string? s = null)
    {
        Go($"Exiting {s}:");
        _wr.Indent = _wr.Indent.Substring(0, _wr.Indent.Length-2);
        _callStack.Pop();
    }

    public static Guid Step() => Step(_callStack.Peek());
    public static Guid Step(Guid id )
    {
        Go($"Step {_counters.Increment(id)}");
        return id;
    }

    public static Guid Start(string name)
    {
        if (!Enabled) return Guid.Empty;
        var id = Guid.NewGuid();
        Go($"Starting task {name}");
        _startTimes[id]= (DateTime.UtcNow, name);
        return id;
    }

    public static void Stop(Guid id)
    {
        if (!Enabled) return;
        var stopTime = DateTime.UtcNow;
        var ticket = _startTimes[id];
        Go($"Task: {ticket.Name} completed at: {stopTime} after ({(stopTime-ticket.Dt).TotalMilliseconds}ms");
        _startTimes.Remove(id);
    }
}