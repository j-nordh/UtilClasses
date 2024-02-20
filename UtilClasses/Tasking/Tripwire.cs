using System;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Extensions.Semaphores;

namespace UtilClasses.Tasking
{

    public class Tripwire<T> where T : IComparable<T>
    {
        private readonly Func<T, T, T> _adder;
        private T? _limit;
        private bool _active;
        protected TaskCompletionSource<T>? _tcs;
        private SemaphoreSlim _semaphore = new(1);
        public Task Task => _tcs!.Task;
        public string Name { get; set; }
        public Tripwire(Func<T, T, T> adder)
        {
            _adder = adder;
            Name = "";

        }

        public Tripwire<T> Set(T val, CancellationToken ct)
        {
            _limit = val;
            _tcs = new TaskCompletionSource<T>(ct);
            _active = true;
            Check();
            return this;
        }

        public void Clear()
        {
            Value = default;
            _active = false;
            _limit = default;
        }
        public T? Value { get; private set; }
        public bool Check(T val)
        {
            if (null == Value)
                Value = val;
            var res = val.CompareTo(Value);
            switch (res)
            {
                case < 0:
                    throw new ArgumentException("Unordered");
                case > 0:
                    _semaphore.On(() => Value = val);
                    break;
            }

            return Check();
        }

        public async Task AddAsync(T val)
        {
            await _semaphore.OnAsync(() =>
            {
                Value = null == Value
                    ? val
                    : _adder(Value, val);
            });
            Check();
        }
        public void Add(T val)
        {
            _semaphore.On(() =>
            {
                Value = null == Value
                    ? val
                    : _adder(Value, val);
            });
            Check();
        }

        private bool Check()
        {
            if (!_active
                || null == _limit
                || null == Value
                || _limit.CompareTo(Value) > 0) return false;

            _active = false;
            _tcs!.SetResult(Value);
            return true;
        }

        public override string ToString() => $"{Name} Active: {_active}, Limit: {_limit}, Value: {Value}";
    }

    public static class Tripwire
    {
        public static Tripwire<int> ForInt() => new((a, b) => a + b);
        public static Tripwire<ulong> ForUlong() => ForUlong("");
        public static Tripwire<ulong> ForUlong(string name) => new((a, b) => a + b) { Name = name };
    }
}