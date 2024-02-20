using System;
using System.Threading;
using System.Threading.Tasks;
using UtilClasses.Extensions.Semaphores;

namespace UtilClasses.Tasking
{

    public class BeanCounter
    {
        private readonly SemaphoreSlim _sem = new(1);
        private TaskCompletionSource<int> _tcs = new();
        private bool _waitForEmpty = false;
        private readonly AsyncValue<int> _balance = new((a, b) => a + b);
        private static IndentingStringBuilder _log = new("  ");

        private int Balance => _balance.Value;

        public ulong Added { get; private set; }

        public ulong Subtracted { get; private set; }

        public async Task AddAsync(int count) => await _sem.OnAsync(() => DoAdd(count));
        public void Add(int count) => _sem.On(() => DoAdd(count));

        public string Name { get; set; }

        public async Task SubtractAsync(int count = 1) => await _sem.OnAsync(() => DoSubtract(count));

        public void Subtract(int count) => _sem.On(() => DoSubtract(count));

        private void DoAdd(int count)//Do not call outside _sem
        {
            _balance.Add(count);
            Added += (ulong)count;
            _log.AppendLine($"{Name}: Added {count}, balance is {Balance}");
        }
        private void DoSubtract(int count) //Do not call outside _sem
        {
            _balance.Add(-count);
            Subtracted += (ulong)count;
            var done = !_waitForEmpty || Balance > 0;
            _log.AppendLine($"{Name}: Subtracted {count}, balance is {Balance}, Done: {done}");
            if (done) return;
            _tcs?.SetResult(0);
            _waitForEmpty = false;
        }

        public static BeanCounter operator ++(BeanCounter b)
        {
            b.Add(1);
            return b;
        }
        public static BeanCounter operator --(BeanCounter b)
        {
            b.Subtract(1);
            return b;
        }

        public async Task Task(CancellationToken ct)
        {
            bool Run()
            {
                if (Balance == 0) return false;
                _tcs = new TaskCompletionSource<int>();
                _waitForEmpty = true;
                return true;
            }
            var wait = await _sem.OnAsync(Run, ct, null);

            if (!wait)
                return;
            await _tcs.Task;
        }
    }
}