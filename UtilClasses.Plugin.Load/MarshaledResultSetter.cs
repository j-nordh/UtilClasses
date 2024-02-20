using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace UtilClasses.Plugins.Load
{
    public class MarshaledResultSetter<T> : MarshalByRefObject
    {
        private readonly TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>();
        public void SetResult(T result) { _tcs.SetResult(result); }
        public Task<T> Task => _tcs.Task;
    }

    public class MarshaledEventPropagator<T> : MarshalByRefObject
    {
        public event Action<string, T> Event;
        public void RaiseEvent(string sender, T arg) => Event?.Invoke(sender, arg);
    }
}
