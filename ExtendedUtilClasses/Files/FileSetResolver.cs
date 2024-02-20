using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UtilClasses;
using UtilClasses.Extensions.DateTimes;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Types;

namespace ExtendedUtilClasses.Files;

public class FileSetResolver : IStreamResolver
{
    private Stream _current;
    private DateTime _started;
    private object _lock = new();
    public int MaxFileSize { get; set; }
    public int MaxFiles { get; set; }
    public string BasePath { get; set; }
    public string NamePattern { get; set; }


    public long CurrentBytes { get; private set; }
    public void Setup(IStreamResolverConfig cfg)
    {
        if(cfg is FileSetResolverConfig fsrc)
            Setup(fsrc);
        else
        {
            throw new ArgumentException($"Tried to initialize a FileSetResolver with a {cfg.GetType().SaneName()}");
        }
    }

    private int _fileCounter;
    private readonly Dictionary<string, Func<string>> _customReplacements = new DictionaryOic<Func<string>>();

    public FileSetResolver()
    {
        OnNewFile = _ => 0;
        OnFileClose = _ => { };
    }


    public Action<Stream> OnFileClose { get; set; }
    public Func<Stream, int> OnNewFile { get; set; }
    public FileSetResolver AddReplacement(string key, Func<string> f) => this.Do(() => _customReplacements[key] = f);
    public FileSetResolver AddReplacement(string key, string s) => this.Do(() => _customReplacements[key] = () => s);
    public virtual void Setup(FileSetResolverConfig cfg)
    {
        MaxFiles = cfg.MaxFiles;
        MaxFileSize = cfg.MaxFileSize;
        BasePath = cfg.BasePath;
        NamePattern = cfg.NamePattern;
        cfg.Replacements.ForEach(t=>_customReplacements[t.Key] = ()=>t.Value);
    }
    public Stream GetStream(int bytes)
    {
        if (null == _lock)
            throw new ObjectDisposedException(GetType().Name, "The writer was accessed after it was disposed");
        if (null != _current)
        {
            if (MaxFileSize <= 0 || CurrentBytes + bytes < MaxFileSize)
            {
                CurrentBytes += bytes;
                return _current;
            }
            OnFileClose?.Invoke(_current);
            _current.Flush();
            _current.Close();
        }
        else
        {
            _started = DateTime.Now;
        }

        if (NamePattern.IsNullOrEmpty())
        {
            File.Open(BasePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
        else
        {
            var started = _started.ToFileString();
            var now = DateTime.Now.ToFileString();
            var kr = new KeywordReplacer()
                .Add("c", _fileCounter % MaxFiles)
                .Add("start", started)
                .Add("starttime", started.SubstringAfter(" "))
                .Add("startdate", started.SubstringBefore(" "))
                .Add("now", now)
                .Add("date", now.SubstringBefore(" "))
                .Add("time", now.SubstringAfter(" "));
            foreach (var kvp in _customReplacements)
                kr.Add(kvp.Key, kvp.Value());

            var path = Path.Combine(BasePath, kr.Run(NamePattern));
            var dir = Path.GetDirectoryName(path);
            if (null == dir)
                if (Directory.Exists(path))
                    dir = path;
                else
                    throw new ArgumentException("The provided path does not contain a directory");

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _current = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            _fileCounter += 1;
        }
        CurrentBytes = OnNewFile(_current);
        return _current;
    }

    public async Task FlushAsync()
    {
        if (null != _current)
            await _current.FlushAsync();
    }
    public void Flush()
    {
        _current?.Flush();
    }

    public void Dispose()
    {
        Flush();
        _current.Dispose();
        _lock = null;
    }

    public async ValueTask DisposeAsync()
    {
        await FlushAsync();
        if (null != _current)
            await _current.DisposeAsync();
        _lock = null;
    }
}

public static class FileSetResolverExtensions
{
    public static T WithConfig<T>(this T r, IStreamResolverConfig cfg) where T : IStreamResolver
    {
        r.Setup(cfg);
        return r;
    }
}