using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace UtilClasses.Dataflow;

public class ArrayReBatcher<T> : ISource<T[]>, ITarget<T[]>
{
    private readonly int _batchSize;
    private readonly BufferBlock<T[]> _outBuffer = new();
    private readonly Queue<T[]> _queue = new();
    private int _storedObjects = 0;
    private ActionBlock<T[]> _handleBlock;
    private T[] _output;
    private int _index;

    public ArrayReBatcher(int batchSize)
    {
        _batchSize = batchSize;
        OutBlock = _outBuffer;
        _handleBlock = new ActionBlock<T[]>(Handle, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
        InBlock = _handleBlock;
    }

    private async Task Handle(T[] arr)
    {
        try
        {
            //fast exit: input data is already a batch
            if (arr.Length == _batchSize && _queue.Count == 0)
            {
                await _outBuffer.SendAsync(arr);
                return;
            }

            //store the array
            _queue.Enqueue(arr);
            _storedObjects += arr.Length;
            if (_storedObjects < _batchSize)
                return;

            await ProcessQueue();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }

    private void Copy(T[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            _output[i + _index] = data[i];
        }
        _index += data.Length;
    }

    private async Task ProcessQueue()
    {
        //we have enough data to forward a batch, but it's not nicely formatted
        //create a new batch
        _output = new T[_batchSize];
        while (true)
        {
            var current = _queue.Dequeue();
            if (_index + current.Length <= _batchSize)
            {
                Copy(current);
                if (_index == _batchSize)
                    break;
                continue;
            }

            //we have more data than we need
            List<T> leftovers = new();
            foreach (var item in current)
            {
                if (_index < _batchSize)
                    _output[_index++] = item;
                else
                {
                    //We have filled the batch, lets put the rest of the objects to the side for a while
                    leftovers.Add(item);
                }
            }

            //the queue must be empty.
            if (_queue.Count > 0)
                throw new Exception(
                    "There is an error in ArrayReBatcher, it seems like there is concurrent execution!");
            _queue.Enqueue(leftovers.ToArray());
            break;
        }

        await _outBuffer.SendAsync(_output);
        _storedObjects -= _batchSize;
        _index = 0;
    }

    public ISourceBlock<T[]> OutBlock { get; }
    public ITargetBlock<T[]> InBlock { get; }
}