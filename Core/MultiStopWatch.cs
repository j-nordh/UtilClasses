using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilClasses.Core;

public class MultiStopWatch
{
    private List<DateTime> _times = new List<DateTime>();
    private List<string> _names = new List<string>();

    public MultiStopWatch(bool started =false)
    {
        if (started)
            Start();
    }
    public void Start() => _times.Add(DateTime.Now);
    public void DoneWith(string name)
    {
        _times.Add(DateTime.Now);
        _names.Add(name);
    }

    public override string ToString() => new IndentingStringBuilder("")
        .AppendLines(_names.Select((n, i) =>$"{n}: {(_times[i + 1] - _times[i]).TotalMilliseconds}ms"))
        .AppendLines($"Total: {(_times.First()-_times.Last()).TotalMilliseconds}ms")
        .ToString();
}