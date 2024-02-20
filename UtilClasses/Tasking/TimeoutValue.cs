using System;
using System.Threading.Tasks;

namespace UtilClasses.Tasking
{
    public class TimeoutValue<T>
    {
        private readonly Func<Task<T>> _refresh;
        private readonly TimeSpan _timeout;
        private DateTime _deadline;
        private T _val;

        public TimeoutValue(Func<Task<T>> refresh, TimeSpan timeout)
        {
            _refresh = refresh;
            _timeout = timeout;
            _deadline = DateTime.MinValue;
        }

        public async Task<T> Get()
        {
            if (DateTime.UtcNow < _deadline) return _val;
            
            _val = await _refresh();
            _deadline = DateTime.UtcNow + _timeout;
            return _val;
        }
    }
}
