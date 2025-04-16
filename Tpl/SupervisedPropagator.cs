//using Common.Interfaces;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;
//using UtilClasses.DataFlow;
//using UtilClasses.Tasking;

//namespace UtilClasses.Dataflow;

//public abstract class SupervisedPropagator<TIn, TOut> : IPropagatorBlock<TIn, TOut>, IPausable
//{
//    public string Name { get; }
//    private BeanCounter _counter = new();
//    protected IPropagatorBlock<TIn, TOut> _prop;
//    private ActionBlock<TIn> _in;
//    private BroadcastBlock<TOut> _out = new(c => c);
//    private ActionBlock<TOut> _outAccounting;
//    private ITargetBlock<TIn> Trg => _in;
//    private ITargetBlock<TIn> Buf => _in;
//    private ISourceBlock<TOut> Src => _out;
//    protected SupervisedPropagator(string name)
//    {
//        Name = name;
//        _counter.Name = $"{name}Counter";
//    }
//    protected void SetupLinks()
//    {
//        _in = new(OnInput, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
//        _outAccounting = new(OnOutput);
//        _prop.LinkTo(_out);
//        _out.LinkTo(_outAccounting);
//    }
//    private void OnOutput(TOut o)
//    {
//        _counter--;
//    }

//    private void OnInput(TIn o)
//    {
//        if (Paused) return;
//        _counter++;
//        _prop.Post(o);
//    }
//    public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader,
//        TIn messageValue,
//        ISourceBlock<TIn> source,
//        bool consumeToAccept) =>
//        Trg.OfferMessage(messageHeader, messageValue, source, consumeToAccept);

//    public void Complete()
//    {
//        _in.Complete();
//        Buf.Complete();
//    }
//    public void Fault(Exception exception) => Buf.Fault(exception);

//    public Task Completion => Src.Completion;

//    public IDisposable LinkTo(ITargetBlock<TOut> target, DataflowLinkOptions linkOptions) =>
//        Src.LinkTo(target, linkOptions);

//    public TOut ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOut> target, out bool messageConsumed)
//    {
//        return Src.ConsumeMessage(messageHeader, target, out messageConsumed);
//    }
//    public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOut> target) => Src.ReserveMessage(messageHeader, target);

//    public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOut> target) => Src.ReleaseReservation(messageHeader, target);

//    public async Task Pause(CancellationToken ct)
//    {
//        Paused = true;
//        await _counter.Task(ct);
//    }

//    public bool Paused { get; private set; }

//    public void Resume()
//    {
//        Paused = false;
//    }
//}

//public static class SupervisedPropagatorExtensions
//{
//    public static Tripwire<T> WithTripwire<T, TIn, TOut>(this SupervisedPropagator<TIn, TOut> prop, Func<TOut, T> f) where T : IComparable<T>
//    {
//        var wire = new Tripwire<T>();
//        prop.LinkToAction(o => wire.Check(f(o)));
//        return wire;
//    }
//}