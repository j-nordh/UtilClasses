using System;
using System.IO;
using System.Threading.Tasks;

namespace ExtendedUtilClasses.Files;

public interface IStreamResolver : IAsyncDisposable , IDisposable
{
    Stream GetStream(int bytes);
    Task FlushAsync();
    void Flush();
    long CurrentBytes { get; }
    void Setup(IStreamResolverConfig cfg);
    string BasePath { get; set; }
}

public enum StreamResolverType
{
    Fileset,
    Simple,
    Mock
}

public interface IStreamResolverConfig
{
    StreamResolverType Type { get; }
}