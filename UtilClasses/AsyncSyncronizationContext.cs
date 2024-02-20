using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilClasses
{
    public class AsyncSynchronizationContext : SynchronizationContext
    {
        private readonly Action<Exception> _handler;

        public AsyncSynchronizationContext(Action<Exception> handler)
        {
            _handler = handler;
        }
        public override void Send(SendOrPostCallback d, object state)
        {
            try
            {
                d(state);
            }
            catch (Exception ex)
            {
                _handler(ex);
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            try
            {
                d(state);
            }
            catch (Exception ex)
            {
                _handler(ex);
            }
        }
    }
}
