using System;
using System.IO;
using System.Threading.Tasks;
using UtilClasses;

namespace ExtendedUtilClasses.Files;

public class MockStreamResolver:IStreamResolver
{
    public PipeStream Stream { get; }

    public MockStreamResolver()
    {
        Stream = new PipeStream();
    }


    public Stream GetStream(int bytes)
    {
        CurrentBytes += bytes;
        return Stream;
    }

    public async Task FlushAsync() => await Stream.FlushAsync();
    public  void Flush() => Stream.Flush();

    [Obsolete]
    public Action<Stream> OnFileClose { get; set; }
    [Obsolete]
    public Func<Stream, int> OnNewFile { get; set; }

    public long CurrentBytes { get; private set; }
    public void Setup(IStreamResolverConfig cfg)
    {
        
    }

    public string BasePath { get; set; }
    public async ValueTask DisposeAsync()
    {
        await FlushAsync();
        await Stream.DisposeAsync();
    }

    public void Dispose()
    {
        Stream?.Dispose();
    }
}

public class MockStreamResolverConfig: IStreamResolverConfig
{
    public StreamResolverType Type => StreamResolverType.Mock;
}
