using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Semaphores;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Tasks;

namespace ExtendedUtilClasses.Files
{
    public class ResolvedStreamWriter : IAsyncDisposable, IDisposable
    {

        public int FlushInterval { get; set; } = 10240;

        protected IStreamResolver _resolver;
        private long _lastFlush;
        private readonly SemaphoreSlim _sem = new(1);
        public long LineCount { get; private set; }
        public ResolvedStreamWriter(FileSetResolverConfig cfg) : this(new FileSetResolver().WithConfig(cfg)) { }

        public ResolvedStreamWriter(IStreamResolver resolver)
        {
            _resolver = resolver;
        }
        public ResolvedStreamWriter(IStreamResolverConfig cfg)
        {
            _resolver = ResolverFactory.FromConfig(cfg);
        }

        private bool FlushNeeded() => _resolver.CurrentBytes - _lastFlush >= FlushInterval;
        public virtual async Task WriteAsync(byte[] bs) => await _sem.OnAsync(async () =>
            {
                var stream = _resolver.GetStream(bs.Length);
                await stream.WriteAsync(bs, 0, bs.Length);
                if (!FlushNeeded()) return;
                await FlushAsync();
            });

        public virtual void Write(byte[] bs) => _sem.On(() =>
            {
                _resolver.GetStream(bs.Length)
                    .Write(bs, 0, bs.Length);
                if (!FlushNeeded()) return;
                Flush();
            });


        private byte[] GetBytes(Encoding enc, params string[] ss)
        {
            var ret = new List<byte>();
            foreach (var s in ss)
            {
                ret.AddRange(s.SplitLines().Select(l => $"{l}\n").SelectMany(enc.GetBytes));
            }
            LineCount += ret.Count;
            return ret.ToArray();
        }
        public virtual void WriteLine(string s, Encoding enc)
        {
            enc ??= Encoding.UTF8;
            var bs = GetBytes(enc, s);
            Write(bs);
        }
        public virtual void WriteLine(params string[] ss)
        {
            var bs = GetBytes(Encoding.UTF8, ss);
            Write(bs);
        }
        public virtual async Task WriteAsync(string s, Encoding enc = null) => await WriteAsync((enc ?? Encoding.UTF8).GetBytes(s));
        public virtual void Write(string s, Encoding enc = null)
        {
            Write((enc ?? Encoding.UTF8).GetBytes(s));
        }
        public virtual void Write(string[] ss) => Write(ss, Encoding.UTF8);
        public virtual void Write(string[] ss, Encoding enc)
        {
            enc ??= Encoding.UTF8;
            var bs = ss.SelectMany(enc.GetBytes).ToArray();
            Write(bs);
        }

        public void Flush()
        {
            _resolver.Flush();
            _lastFlush = _resolver.CurrentBytes;
        }
        public virtual async Task FlushAsync()
        {
            await _resolver.FlushAsync();
            _lastFlush = _resolver.CurrentBytes;
        }

        public virtual void Setup(FileSetResolverConfig cfg)
        {
            _resolver.Setup(cfg);
            FlushInterval = cfg.FlushInterval;
        }

        public virtual async ValueTask DisposeAsync()
        {
            await _resolver.DisposeAsync();
        }

        public virtual void Dispose()
        {
            _resolver?.Dispose();
        }
    }

    public class ResolvedStreamWriter<T> : ResolvedStreamWriter
    {
        private readonly Func<T, byte[]> _serializer;

        public ResolvedStreamWriter(IStreamResolver resolver, Func<T, byte[]> serializer) : base(resolver)
        {
            _serializer = serializer;
        }
        public ResolvedStreamWriter(IStreamResolverConfig cfg, Func<T, byte[]> serializer) : base(cfg)
        {
            _serializer = serializer;
        }
        public void Write(T o) => Write(new[] { o });
        public void Write(IEnumerable<T> os) => os.Select(_serializer).ForEach(Write);
        public async Task WriteAsync(IEnumerable<T> os)
        {
            var tasks = os.Select(_serializer).Select(WriteAsync).ToList();
            await tasks.WhenAny();
        }
        public async Task WriteAsync(T o) => await WriteAsync(new[] { o });
    }

    public static class ResolvedStreamWriterExtensions
    {
        public static async Task MaybeFlushAsync(this ResolvedStreamWriter wr)
        {
            if (null == wr) return;
            await wr.FlushAsync();
        }
        public static async Task MaybeFlushAsync(this StreamWriter wr)
        {
            if (null == wr) return;
            await wr.FlushAsync();
        }
        public static async Task MaybeFlushAsync(this PipeStream wr)
        {
            if (null == wr) return;
            await wr.FlushAsync();
        }
        
    }

    public static class FileSetWriterExtensions
    {
        public static T WithConfig<T>(this T fileSetWriter, FileSetResolverConfig cfg) where T : ResolvedStreamWriter
        {
            fileSetWriter.Setup(cfg);
            return fileSetWriter;
        }
        public static T WithConfig<T>(this T fileSetWriter, string basePath, string namePattern) where T : ResolvedStreamWriter
        {
            fileSetWriter.Setup(FileSetResolverConfig.Default(basePath, namePattern));
            return fileSetWriter;
        }
    }
}
