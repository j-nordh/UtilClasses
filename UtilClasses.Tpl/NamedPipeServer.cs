using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Common.Interfaces;
using UtilClasses.Extensions.Tasks;

namespace UtilClasses.Dataflow;

public class NamedPipeServer<T, TPacker> : IDisposable, ISource<T> where TPacker : IBytePacker<T>
{
    private NamedPipeServerStream _serverPipe;
    private Task _readTask;
    private bool _terminate;
    private CancellationTokenSource _readCts;
    public event EventHandler<Exception> OnReadBlockException;
    public event EventHandler<Exception> OnReadException;
    private BroadcastBlock<T> _outBlock;
    private readonly TPacker _packer;

    public int ReadBufferSize { get; set; }
    public string PipeName { get; }

    public NamedPipeServer(string pipeName, TPacker packer, Func<T, T> cloneFunc = null)
    {
        _packer = packer;
        PipeName = pipeName;
        ReadBufferSize = 1024;
        _outBlock = new BroadcastBlock<T>(cloneFunc ?? (x => x));
    }

    public void Dispose()
    {
        _serverPipe.Dispose();
    }

    public ISourceBlock<T> OutBlock => _outBlock;

    public Task SetupRead()
    {
        _serverPipe = new NamedPipeServerStream(PipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte);
        _serverPipe.WaitForConnectionAsync().Forget();
        return Task.FromResult(0);
    }
    public void StartRead()
    {
        if (null != _readTask) throw new InvalidOperationException("This NamedPipeManager is already reading!");
        _terminate = false;
        _readCts = new CancellationTokenSource();
        _readTask = Task.Run(ReadLoop);
    }

    public void StopRead()
    {
        if (null == _readTask) return;
        _terminate = true;
        _readCts.Cancel();
        //await _readTask;
    }

    private async Task ReadLoop()
    {
        var buffer = new byte[ReadBufferSize];
        var offset = 0;
        int exceptions = 0;
        while (true)
        {
            try
            {
                var limit = await _serverPipe.ReadAsync(buffer, offset, buffer.Length - offset, _readCts.Token).OnFault(e =>
                {
                    if (!_terminate) throw e;
                    return -1;
                });
                if (_terminate) break;

                var (bytesRead, exCount) = ReadBlock(buffer, limit);
                exceptions = exCount;
                if (bytesRead == limit)
                {
                    offset = 0;
                    continue;
                }

                var left = limit - bytesRead;
                Buffer.BlockCopy(buffer, bytesRead, buffer, 0, left);
                offset = left;
            }
            catch (Exception e)
            {
                OnReadException?.Invoke(this, e);
            }

            if (exceptions > 0)
            {
                OnReadException?.Invoke(this, new Exception($"Block read threw {exceptions} exceptions during last read."));
            }
        }
    }

    (int bytes, int exceptions) ReadBlock(byte[] buffer, int limit)
    {
        int i = 0;
        var exceptions = 0;
        while (i < limit)
        {
            try
            {
                var res = _packer.Unpack(buffer, i, out var o);
                switch (res)
                {
                    case UnpackResult.Ok:
                        _outBlock.Post(o);
                        i += _packer.ObjectSize;
                        continue;
                    case UnpackResult.Corrupt:
                        i += 1;
                        continue;
                    case UnpackResult.NotEnoughData:
                        return (i, exceptions);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                OnReadBlockException?.Invoke(this, e);
                i += 1;
                exceptions += 1;
            }
        }

        return (i, exceptions);
    }
}