using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ExtendedUtilClasses.Files;
using UtilClasses.Files;

namespace ExtendedUtilClasses
{
    public class BackgroundResolvedStreamWriter<T> : ResolvedStreamWriter<T>, IAsyncDisposable, IDisposable
    {
        private readonly BlockingCollection<byte[]> _q = new(new ConcurrentQueue<byte[]>());
        private TaskRunner _tr;

        public override void Setup(FileSetResolverConfig cfg)
        {
            base.Setup(cfg);
            _tr = new TaskRunner(Writer, false);
            if (!_tr.Running)
                _tr.Start();
        }

        public string BasePath
        {
            get => _resolver.BasePath; set => _resolver.BasePath = value;
        }
        public override void Write(byte[] bs) => _q.Add(bs);

        private async Task Writer()
        {
            var itm = _q.Take();
            if (null == itm) return;
            base.Write(itm);
            await Task.FromResult(0);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if(null!= _tr)
                await _tr.DisposeAsync();
        }

        public BackgroundResolvedStreamWriter(IStreamResolver res, Func<T, byte[]> serializer) : base(res, serializer)
        {
            
        }

        public override void Dispose()
        {
            _q?.Dispose();
        }
    }
    public class BackgroundResolvedStreamWriter : BackgroundResolvedStreamWriter<byte[]>
    {
        private readonly BlockingCollection<byte[]> _q = new(new ConcurrentQueue<byte[]>());
        private TaskRunner _tr;

        public override void Setup(FileSetResolverConfig cfg)
        {
            base.Setup(cfg);
            _tr = new TaskRunner(Writer, false);
            if (!_tr.Running)
                _tr.Start();
        }

        public override void Write(byte[] bs) => _q.Add(bs);

        private async Task Writer()
        {
            var itm = _q.Take();
            if (null == itm) return;
            base.Write(itm);
            await Task.FromResult(0);
        }

        public BackgroundResolvedStreamWriter(IStreamResolver res) : base(res, bs=>bs)
        {

        }
    }
}
