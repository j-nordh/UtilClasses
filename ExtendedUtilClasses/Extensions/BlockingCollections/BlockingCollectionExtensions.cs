using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedUtilClasses.Extensions.BlockingCollections
{
    public static class BlockingCollectionExtensions
    {
        public static async Task<T> TakeAsync<T>(this BlockingCollection<T> q, T defaultValue)
            => await q.TakeAsync(defaultValue, null);

        public static async Task<T> TakeAsync<T>(this BlockingCollection<T> q) where T : class
            => await q.TakeAsync(null, null);
        public static async Task<T> TakeAsync<T>(this BlockingCollection<T> q, CancellationTokenSource cts) where T : class
            => await q.TakeAsync(null, cts);
        public static async Task<T> TakeAsync<T>(this BlockingCollection<T> q, T defaultValue, CancellationTokenSource cts) 
        {
            var t = cts == null
                ? Task.Run(q.Take)
                : Task.Run(q.Take, cts.Token);
            return await t.ContinueWith(t => (t.IsCompletedSuccessfully) ? t.Result : defaultValue);
        }
    }
}
