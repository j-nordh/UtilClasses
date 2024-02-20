using System;
using System.Threading;
using System.Threading.Tasks;

namespace UtilClasses.Extensions.Concurrency
{
    public static class ReaderWriterLockExtensions
    {
        public static bool InRead(this ReaderWriterLockSlim @lock, Action f) => @lock.In(false, () => { f(); return Task.FromResult(true); }).Result;
        public static bool InWrite(this ReaderWriterLockSlim @lock, Action a) => @lock.In(true, () => { a(); return Task.FromResult(true); }).Result;
        public static T InRead<T>(this ReaderWriterLockSlim @lock, Func<T> f) => @lock.In(false, () => Task.FromResult(f())).Result;
        public static T InWrite<T>(this ReaderWriterLockSlim @lock, Func<T> f) => @lock.In(true, () => Task.FromResult(f())).Result;
        public static Task InWrite(this ReaderWriterLockSlim @lock, Func<Task> f) => @lock.In(true, f).ContinueWith(t => t != null);
        public static Task InRead(this ReaderWriterLockSlim @lock, Func<Task> f) => @lock.In(false, f).ContinueWith(t => t != null);
        public static Task InRead<T>(this ReaderWriterLockSlim @lock, Func<Task<T>> f) => @lock.In(false, f).ContinueWith(t => t != null);
        public static Task InWrite<T>(this ReaderWriterLockSlim @lock, Func<Task<T>> f) => @lock.In(true, f).ContinueWith(t => t != null);

        private static async Task In(this ReaderWriterLockSlim @lock, bool write, Func<Task> f, int timeout = 1000) =>
            await @lock.In<bool>(write,
                async () =>
                {
                    await f();
                    return true;
                }, timeout);
        private static async Task<T> In<T>(this ReaderWriterLockSlim @lock, bool write, Func<Task<T>> f, int timeout = 1000)
        {
            var gotLock = write ? @lock.TryEnterWriteLock(timeout) : @lock.TryEnterReadLock(timeout);
            if (!gotLock) throw new TimeoutException($"Could not acquire lock within {timeout}ms.");
            T ret;
            try
            {
                ret = await f();
            }
            finally
            {
                if (write)
                    @lock.ExitWriteLock();
                else
                    @lock.ExitReadLock();
            }
            return ret;
        }

    }
}
