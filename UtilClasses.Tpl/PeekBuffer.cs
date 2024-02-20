using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace UtilClasses.Dataflow
{
    public class PeekBuffer<T> : ISource<T>, ITarget<T>, IDisposable
    {
        public ISourceBlock<T> OutBlock { get; }
        public ITargetBlock<T> InBlock { get; }
        private List<IDisposable> _links = new();

        public PeekBuffer(Action<T> a)
        {
            var inBuffer = new BufferBlock<T>();
            var trans = new TransformBlock<T, T>(o =>
            {
                a(o);
                return o;
            });
            var outBuffer = new BufferBlock<T>();
            _links.AddRange(new[]{
                inBuffer.LinkTo(trans),
                trans.LinkTo(outBuffer),
            });
            InBlock = inBuffer;
            OutBlock = outBuffer;
        }

        public void Dispose()
        {
            _links.ForEach(l=>l.Dispose());
        }
    }
}
