//using System.Threading.Tasks.Dataflow;

//namespace UtilClasses.Dataflow;

//public class SupervisedBufferBlock<T>  : SupervisedPropagator<T,T>
//{
//    public SupervisedBufferBlock(string name) : base(name)
//    {
//        _prop = new BufferBlock<T>();
//        SetupLinks();
//    }
//    public SupervisedBufferBlock(string name, DataflowBlockOptions opt) : base(name)
//    {
//        _prop = new BufferBlock<T>(opt);
//        SetupLinks();
//    }
//}