using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UtilClasses.Extensions.DateTimes;
using UtilClasses.Extensions.Types;

namespace ExtendedUtilClasses.Files;

public class SimpleStreamResolver : IStreamResolver
{
    private Stream _stream;
    private static DateTime _lastDate = DateTime.MinValue;
    public static List<string> AssignedPaths { get; } = new();
    public async ValueTask DisposeAsync()
    {
        await _stream.DisposeAsync();
    }

    public void Dispose()
    {
        _stream.Dispose();
    }

    public Stream GetStream(int bytes)
    {
        CurrentBytes += bytes;
        return _stream;
    }

    public async Task FlushAsync()
    {
        await _stream.FlushAsync();
    }

    public void Flush()
    {
        _stream.Flush();
    }

    public long CurrentBytes { get; private set; }
    public void Setup(IStreamResolverConfig config)
    {
        if (config is not SimpleStreamResolverConfig cfg)
            throw new ArgumentException(
                $"Tried to configure a {GetType().SaneName()} with a {config.GetType().SaneName()} object.");

        var path = cfg.Path;
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        if (cfg.PrefixDateTime)
        {
            if ((DateTime.UtcNow - _lastDate).TotalSeconds > 5)
                _lastDate = DateTime.UtcNow;
            path = Path.Combine(dir, $"{_lastDate.ToFileString()}_{Path.GetFileName(path)}");
        }

        if (File.Exists(path))
        {
            var tmp = path;
            int i = 2;
            do
            {
                tmp = $"{Path.GetFileNameWithoutExtension(path)}_{i}{Path.GetExtension(path)}";
            } while (File.Exists(tmp));

            path = tmp;
        }
        BasePath = path;
        lock(AssignedPaths)
            AssignedPaths.Add(path);
        _stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
    }

    public static DateTime PresetPrefixTime()
    {
        _lastDate = DateTime.UtcNow;
        return _lastDate;
    }

    public static DateTime CurrentPresetTime => _lastDate;
    public string BasePath { get; set; }
}

public class SimpleStreamResolverConfig : IStreamResolverConfig
{
    public StreamResolverType Type => StreamResolverType.Simple;
    public string Path { get; set; }
    public bool PrefixDateTime { get; set; }
    public bool PostfixNumber { get; set; }
}