using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilClasses;

public class AsymmetricBuffer<T>
{
    private readonly Queue<T[]> _q = new();
    private T[]? _current = {};
    private int _currentIndex = 0;
    private readonly object _currentLock = new();
    private int _totalLength;
    public void Write(T[] buffer, int offset, int count)
    {
        if (offset == 0 && count == buffer.Length)
        {
            lock (_q)
                _q.Enqueue(buffer);
            _totalLength += count;
            return;
        }

        var arr = new T[count];
        Array.Copy(buffer, offset, arr, 0, count);
        lock (_q)
            _q.Enqueue(arr);
        _totalLength += count;
    }

    public void Write(T[] buffer) => Write(buffer, 0, buffer.Length);

    public int Read(T[] buffer, int offset, int count)
    {
        var totalLength = 0;

        while (true)
        {
            var len = ReadCurrent(buffer, offset, count);
            offset += len;
            count -= len;
            totalLength += len;
            if (count <= 0 || len == 0)
            {
                _totalLength -= totalLength;
                return totalLength;
            }
                
        }
    }

    public int Length => _totalLength;

    public int Read(int count, out T[] buffer)
    {
        buffer = new T[count];
        var ret =  Read(buffer, 0, count);
        _totalLength -= ret;
        return ret;
    }

    private int ReadCurrent(T[] buffer, int offset, int count)
    {
        lock (_currentLock)
        {
            if (null == _current)
            {
                lock (_q)
                {
                    if (_q.Any())
                        _current = _q.Dequeue();
                    return 0;
                }
            }

            var length = new[] { count, _current.Length - _currentIndex }.Min();
            Array.Copy(_current, _currentIndex, buffer, offset, length);
            _currentIndex += length;
            if (_currentIndex >= _current.Length)
            {
                _currentIndex = 0;
                _current = null;
            }

            return length;
        }

    }
}