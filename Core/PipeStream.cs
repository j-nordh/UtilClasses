using System;
using System.IO;
using System.Text;

namespace UtilClasses.Core;

public class PipeStream : Stream
{
    private readonly MemoryStream _inner;
    private long _readPosition;
    private long _writePosition;

    public PipeStream()
    {
        _inner = new MemoryStream();
    }
    public PipeStream(string content) : this()
    {
        using (var wr = new StreamWriter(_inner, Encoding.UTF8, 10240, true))
            wr.Write(content);
        _writePosition = _inner.Position;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override void Flush()
    {
        lock (_inner)
        {
            _inner.Flush();
        }
    }

    public void Clear()
    {


    }
    public override long Length
    {
        get
        {
            lock (_inner)
            {
                return _inner.Length;
            }
        }
    }

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public int Available => (int)(_writePosition - _readPosition);

    public override int Read(byte[] buffer, int offset, int count)
    {
        lock (_inner)
        {
            _inner.Position = _readPosition;
            int read = _inner.Read(buffer, offset, count);
            _readPosition = _inner.Position;

            return read;
        }
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        lock (_inner)
        {
            _inner.Position = _writePosition;
            _inner.Write(buffer, offset, count);
            _writePosition = _inner.Position;
        }
    }
}