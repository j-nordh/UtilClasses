//using System;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;

//namespace UtilClasses.Dataflow
//{
//    public class SupervisedTransformBlock<TIn, TOut> : SupervisedPropagator<TIn,TOut>
//    {
//        public SupervisedTransformBlock(string name, Func<TIn, TOut> f) : base(name)
//        {
//            _prop = new TransformBlock<TIn, TOut>(f);
//            SetupLinks();
//        }
//        public SupervisedTransformBlock(string name, Func<TIn, Task<TOut>> f) : base(name)
//        {
//            _prop = new TransformBlock<TIn, TOut>(f);
//            SetupLinks();
//        }
//    }
//}
