using System;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Extensions.Semaphores;

namespace UtilClasses;

public class Ticker
{
    private bool _running;
    private Task? _task;
    private TimeSpan _period;
    private DateTime _nextTick;
    private Func<DateTime, Task> _onTick;
    public string Name { get; }
    private SemaphoreSlim _semaphore = new(1);

    public Ticker(string name, TimeSpan period, Func<DateTime, Task> onTick)
    {
        _onTick = onTick;
        _period = period;
        Name = name;
    }

    public void Start(DateTime? startTime = null)
    {
        _running = true;
        _nextTick = startTime ?? DateTime.UtcNow;
        _task = Task.Run(Run);
    }

    public async Task<bool> Running()
    {
        if (!_running || null == _task) return false;
        if (_task.IsCompleted) return false;
        if (_task.IsCanceled) return false; //belt and suspenders, should be handled by _running...
        if (_task.IsFaulted) await _task; //this should throw something
        return true;
    }

    private async Task Run()
    {
        async Task DoIt()
        {
            try
            {
                if (DateTime.UtcNow < _nextTick)
                {
                    await Task.Delay(100);
                    return;
                }
                    
                await _onTick(_nextTick);
                _nextTick += _period;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        while (_running)
        {
            try
            {
                await _semaphore.OnAsync(DoIt);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public async Task Stop()
    {
        _running = false;
        if (null != _task)
            await _task;
    }
}