using System;
using UtilClasses.Extensions.Types;

namespace ExtendedUtilClasses.Files;

public static class ResolverFactory
{
    public static IStreamResolver FromConfig(IStreamResolverConfig cfg) => cfg switch
    {
        FileSetResolverConfig fs => new FileSetResolver().WithConfig(fs),
        MockStreamResolverConfig ms => new MockStreamResolver().WithConfig(ms),
        SimpleStreamResolverConfig ss=> new SimpleStreamResolver().WithConfig(ss),
        _ => throw new ArgumentException($"Could not construct a StreamResolver for {cfg.GetType().SaneName()}")
    };

    public static FileSetResolver FileSet(string basePath, string namePattern = null) => (FileSetResolver)
        FromConfig(FileSetResolverConfig.Default(basePath, namePattern));

}

public static class WriterFactory
{
    public static ResolvedStreamWriter FileSet(string basePath, string namePattern = null) =>
        new(ResolverFactory.FileSet(basePath, namePattern));

    public static ResolvedStreamWriter FromConfig(IStreamResolverConfig cfg, bool runInBackground=false) =>
        runInBackground 
            ? new BackgroundResolvedStreamWriter(ResolverFactory.FromConfig(cfg))
            : new (ResolverFactory.FromConfig(cfg));

    public static ResolvedStreamWriter<T> FromConfig<T>(IStreamResolverConfig cfg, Func<T, byte[]> serializer, bool runInBackground) =>
        runInBackground
            ? new BackgroundResolvedStreamWriter<T>(ResolverFactory.FromConfig(cfg), serializer)
            : new(ResolverFactory.FromConfig(cfg), serializer);

    public static ResolvedStreamWriter<T> FromConfig<T>(IStreamResolverConfig cfg, Func<T, byte[]> packer) =>
        new(ResolverFactory.FromConfig(cfg), packer);
}
