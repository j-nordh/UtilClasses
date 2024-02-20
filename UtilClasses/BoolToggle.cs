using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses
{
    public class BoolToggle : Toggle<bool>
    {
        public Toggler<bool> Get(bool start = true) => Get(!Value, Value);
    }

    public class Toggle<T>
    {
        public T Value { get; set; }
        public Toggler<T> Get(T start, T end) => new Toggler<T>(this, start, end);
    }
    public class Toggler<T> : IDisposable
    {
        private Toggle<T> _toggle;
        readonly T _end;
        public Toggler(Toggle<T> toggle, T start, T end)
        {
            _end = end;
            _toggle = toggle;
            _toggle.Value = start;
        }
        public void Dispose()
        {
            _toggle.Value = _end;
        }
    }
}
