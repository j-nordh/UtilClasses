using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UtilClasses.Core.Files;

[Obsolete]
public class MultiFileWriter : IDisposable
{
    private readonly string _dir;
    private readonly string _namePattern;
    public int MaxLines { get; set; }
    public int MaxFiles { get; set; }
    public int MaxBytes { get; set; }
    private StreamWriter? _writer;
    public int FileCount { get; private set; }
    public int FileLineCount { get; private set; }
    public int LineCount { get; private set; }
    public long FileBytesCount { get; private set; }
    public bool OverWrite { get; set; }

    private bool _disposed = false;

    public event Action? OnFull;
    public bool IsFull { get; set; }


    public MultiFileWriter(string dir, string namePattern)
    {
        _dir = dir;
        _namePattern = namePattern; //"Test_%c%" "Test_%timestamp%"
        MaxLines = -1;
        MaxFiles = -1;
        MaxBytes = -1;
        FileCount = -1;
        OverWrite = false;
        IsFull = false;
    }

    public void Write(IEnumerable<string> lines)
    {
        var wr = GetWriter();

        if (IsFull) return;

        foreach (var line in lines)
        {
            wr.WriteLine(line);
            FileLineCount += 1;
            LineCount += 1;
        }
        wr.Flush();

        FileBytesCount = wr.BaseStream.Length;
    }

    public void Write(string s)
    {
        var wr = GetWriter();

        if (IsFull) return;

        wr.Write(s);
        var lines = s.Count(c => c == '\n');

        FileLineCount += lines;
        LineCount += lines;

        wr.Flush();

        FileBytesCount = wr.BaseStream.Length;
    }

    public void Write(byte[] bytes)
    {
        var wr = GetWriter();

        if (IsFull) return;

        wr.BaseStream.Write(bytes, 0, bytes.Length);

        //Cannot really count lines here

        wr.Flush();
        FileBytesCount = wr.BaseStream.Length;
    }


    private StreamWriter GetWriter()
    {
        if (IsFull) return null;

        bool currentOk = _writer != null;

        if (currentOk)
        {
            if (MaxLines > 0 && FileLineCount > MaxLines) currentOk = false;
            if (MaxBytes > 0 && FileBytesCount > MaxBytes) currentOk = false;

        }
        if (currentOk) return _writer;

        _writer?.Close();
        FileCount += 1;

        if (!OverWrite && MaxFiles > 0 && FileCount > MaxFiles)
        {
            OnFull?.Invoke();
            IsFull = true;
            return null;
        }

        var replacer = new KeywordReplacer()
            .Add("c", (MaxFiles > 0 ? FileCount % MaxFiles : FileCount).ToString())
            .Add("timestamp", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        var filePath = Path.Combine(_dir, replacer.Run(_namePattern));

        if (OverWrite && File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _writer = new StreamWriter(filePath);
        FileLineCount = 0;
        return _writer;
    }

    private string GetTimestamp()
    {
        return DateTime.Now.ToString("yyyyMMdd_HHmmss");
    }

    public void Dispose()
    {
        Dispose(true);
        //GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _writer?.Close();
        }

        _disposed = true;
    }
}