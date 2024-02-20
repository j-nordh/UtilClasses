using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses
{
    public class MonitoredValue<T> where T:IEquatable<T>
    {
        private T _val;

        public MonitoredValue(T val)
        {
            _val = val;
        }

        public event EventHandler<T>? Changed;

        public T Value
        {
            get => _val;
            set
            {
                if (_val.Equals(value)) return;
                _val = value;
                Changed?.Invoke(this, _val);
            }
        }

        public static implicit operator T(MonitoredValue<T> mv) => mv._val;
    }
}
