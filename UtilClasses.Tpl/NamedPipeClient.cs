using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Common.Interfaces;
using UtilClasses.Extensions.Bytes;

namespace UtilClasses.Dataflow
{
    public class NamedPipeClient<T, TPacker> : IDisposable, ITarget<T> where TPacker : IBytePacker<T>
    {
        private readonly TPacker _packer;
        private NamedPipeClientStream _clientPipe;
        private IDisposable _link;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeName">A unique name for this connection</param>
        /// <param name="readFunc">A function that reads an object from a byte buffer, if the buffer does not contain the whole object it should return null</param>
        public NamedPipeClient(string pipeName, TPacker packer, int batchSize=-1)
        {
            _packer = packer;
            PipeName = pipeName;

            if (batchSize < 2)
                InBlock = new ActionBlock<T>(async o => await Write(o));
            else
            {
                var bb = new BatchBlock<T>(batchSize);
                InBlock = bb;
                _link = bb.LinkTo(new ActionBlock<T[]>(async os => await Write(os)));
            }
                
        }

        public string PipeName { get; }

        public async Task SetupWrite()
        {
            _clientPipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
            await _clientPipe.ConnectAsync();
        }

       

        public async Task Write(byte[] buffer, int offset = 0, int count = -1)
        {
            if (count < 0) count = buffer.Length - offset;
            await _clientPipe.WriteAsync(buffer, offset, count);
            _clientPipe.Flush();
        }

        public async Task Write(T obj) => await Write(_packer.Pack(obj));

        public async Task Write(T[] os)
        {
            await Write(_packer.Pack(os));
        }
        
        public void Dispose()
        {
            _clientPipe.Close();
            _link.Dispose();
        }

        public ITargetBlock<T> InBlock { get; }
    }
}