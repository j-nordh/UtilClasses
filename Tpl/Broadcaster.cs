using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Transactions;
using UtilClasses.Core.Tasking;

namespace UtilClasses.Dataflow;

public class Broadcaster<T> : ITarget<T>, ISource<T>, IDisposable
{
    public string Name { get; set; }
    private readonly BroadcastBlock<T> _bb;
    private readonly BufferBlock<T> _buffer = new();
    private readonly LinkKeeper _links = new();
    private readonly AsyncValue<ulong> _counter = AsyncValue.ForUlong();
    public Broadcaster(Func<T, T> cloningFunc = null, bool debug = false) : this("unnamed", cloningFunc, debug){}
    public Broadcaster(string name, Func<T, T> cloningFunc = null, bool debug = false)
    {
        Name = name;
        cloningFunc ??= x => x;
        _bb = new BroadcastBlock<T>(cloningFunc);
        
        _links.Peek(_buffer,BookKeeping,_bb);

        InBlock = _buffer;
        OutBlock = _bb;

    }

    private async Task BookKeeping(T arg)
    {
        await _counter.AddAsync(1);
    }

    public ITargetBlock<T> InBlock { get; }
    public ISourceBlock<T> OutBlock { get; }
    public ulong Counter => _counter.Value;

    public void Dispose()
    {
        _links?.Dispose();
    }
}