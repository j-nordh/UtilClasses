using System;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Extensions.Semaphores;

namespace UtilClasses.Tasking
{

    public class AsyncValue<T>
    {
        private readonly SemaphoreSlim _semaphore = new(1);

        private T? _value;
        private readonly Func<T, T, T>? _adder;
        public object? Tag { get; set; }
        public AsyncValue(Func<T, T, T> adder)
        {
            _adder = adder;
        }
        public T? Value => _value;

        public async Task SetAsync(T value) => await _semaphore.OnAsync(() => _value = value, 100);
        public void Set(T value) => _semaphore.On(() => _value = value);
        public async Task AddAsync(T val)
        {
            if (null == _adder)
                throw new ArgumentException("No adder has been specified");

            void ExceptionHandler(Exception ex) => Console.WriteLine("Exception in AsyncValue AddAsync");
            await _semaphore.OnAsync(() => _value = _value == null
                ? val
                : _adder(_value, val), 100, ExceptionHandler);
        }

        public void Add(T val)
        {
            if (null == _adder)
                throw new ArgumentException("No adder has been specified");
            _semaphore.On(() => _value == null
                ? val
                : _adder(_value, val));
        }

        public override string ToString() => _value?.ToString() ?? "";
    }
    public static class AsyncValue
    {
        public static AsyncValue<int> ForInt() => new((a, b) => a + b);
        public static AsyncValue<ulong> ForUlong() => new((a, b) => a + b);
    }
}
