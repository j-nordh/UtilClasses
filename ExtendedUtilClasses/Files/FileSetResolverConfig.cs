using Common.Interfaces;
using UtilClasses;
using UtilClasses.Extensions.Dictionaries;

namespace ExtendedUtilClasses.Files;

public class FileSetResolverConfig : ICloneable<FileSetResolverConfig>, IStreamResolverConfig
{
    public int MaxFileSize { get; set; }
    public int MaxFiles { get; set; }
    public string BasePath { get; set; }
    public string NamePattern { get; set; }
    public int FlushInterval { get; set; } = 10240;
    public DictionaryOic<string> Replacements { get; set; }
    public StreamResolverType Type => StreamResolverType.Fileset;
    public FileSetResolverConfig() { }

    public FileSetResolverConfig(FileSetResolverConfig o)
    {
        MaxFileSize = o.MaxFileSize;
        MaxFiles = o.MaxFiles;
        BasePath = o.BasePath;
        NamePattern = o.NamePattern;
        FlushInterval = o.FlushInterval;
        Replacements = o.Replacements.ToDictionaryOic();
    }

    public FileSetResolverConfig Clone() => new(this);

    public static FileSetResolverConfig Default(string basePath, string namePattern = null) => new()
    {
        MaxFileSize = 10485760,
        MaxFiles = 10,
        BasePath = basePath,
        NamePattern = namePattern?? "Log_%c%.log",
        FlushInterval = 102400
    };

    
}

public static class FileSetWriterConfigExtensions
{
    public static FileSetResolverConfig WithReplacement(this FileSetResolverConfig cfg, string key, string val)
    {
        cfg.Replacements.Add((key, val));
        return cfg;
    }
}