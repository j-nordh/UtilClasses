using System;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Extensions.Tasks;

namespace ExtendedUtilClasses
{
    public class TaskRunner:IAsyncDisposable
    {
        protected Func<Task> _func;
        public int MaxIterations { get; set; }
        private bool _cancel;
        private Task _runner;
        public int BreathSpacing { get; set; } = 1000;
        public int Delay { get; set; }
        public bool AdjustDelay { get; set; }
        public event Action<Exception> ExceptionCaught;
        public event Action Completed;
        public int Iteration { get; private set; }
        public CancellationTokenSource Cts { get; private set; }
        private bool _started;
        public TaskRunner(Func<Task> func, bool start=true)
        {
            _func = func;
            MaxIterations = -1;
            if(start)
                Start();
        }

        async Task Run()
        {
            Task innerTask=null;
            Cts = new CancellationTokenSource();
            while (!_cancel)
            {
                try
                {
                    var start = DateTime.UtcNow;
                    innerTask ??= _func();
                    await new[] { Task.Delay(BreathSpacing), innerTask}.WhenAny();
                    Iteration += 1;
                    if (innerTask.IsRunning()) continue;
                    innerTask = null;
                    if (MaxIterations > 0 && Iteration >= MaxIterations) break;

                    if (Delay > 0)
                    {
                        await Task.Delay(AdjustDelay? Delay - (int)(DateTime.UtcNow - start).TotalMilliseconds : Delay);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionCaught?.Invoke(ex);
                }
            }
            Completed?.Invoke();
        }

        public bool Running => _started && !_cancel;

        public void Start()
        {
            _cancel = false;
            _runner = Task.Run(Run);
            _started = true;
        }

        public async Task Stop()
        {
            _cancel = true;
            Cts.Cancel();
            await _runner;
            _runner = null;
        }

        public async ValueTask DisposeAsync()
        {
            await Stop();
        }

    }

    public class TaskRunner<T> : TaskRunner
    {
        public T Value { get; set; }
        public event Action<T> GotValue; 
        public Func<T, bool> ExitCondition { get; set; }
        public TaskRunner(Func<Task<T>> func, bool start = true) : base(null, false)
        {
            _func = () => func().ContinueWith(async t =>
            {
                if (t.IsFaulted)
                    throw t.Exception ?? new Exception("TaskRunner: Task is faulted but no exception is provided");
                if (!t.IsCompletedSuccessfully) return; 
                
                Value = t.Result;
                GotValue?.Invoke(Value);
                if (ExitCondition?.Invoke(Value) ?? false)
                    await Stop();
            });
            if (start) Start();
        }
    }
}
