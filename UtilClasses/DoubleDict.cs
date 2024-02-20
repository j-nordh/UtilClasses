using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Dictionaries;

namespace UtilClasses
{
    public  class DoubleDict<T1,T2>where T1:IComparable
    {
        private Dictionary<T1, T2> _forward;
        private Dictionary<T2, T1> _reverse;
        public List<(T1 a, T2 b)> State
        {
            get => _forward.Select(kvp => (kvp.Key, kvp.Value)).ToList();
            set
            {
                _forward.Clear();
                _reverse.Clear();
                _maxA = default(T1);
                value.ForEach(x => Insert(x.a, x.b));
            }
        }

        T1 _maxA;

        public Func<T1, T1>? Increment { get; set; }

        public DoubleDict(IEqualityComparer<T1>? cmpA, IEqualityComparer<T2>? cmpB)
        {
            _forward = cmpA ==null? new Dictionary<T1, T2>(): new Dictionary<T1, T2>(cmpA);
            _reverse = cmpB == null ? new Dictionary<T2, T1>() : new Dictionary<T2, T1>(cmpB);
        }
        public DoubleDict(IEqualityComparer<T1>? cmpA) : this(cmpA, null)
        { }
        public DoubleDict(IEqualityComparer<T2>? cmpB) : this(null, cmpB)
        { }
        public DoubleDict() : this(null, null)
        { }

        public DoubleDict(IEnumerable<(T1,T2)> items, IEqualityComparer<T1>? cmpA = null, IEqualityComparer<T2>? cmpB = null) : this(cmpA, cmpB)
        {
            foreach(var (a, b) in items)
            {
                _forward[a] = b;
                _reverse[b] = a;
            }
        }

        

        public T1 GetOrAdd(T2 val)
        {
            if (null == Increment)
                throw new ArgumentNullException("Cannot use GetOrAdd without specifying \"Increment\"");
            return _reverse.TryGetValue(val, out var ret)
                ? ret
                : Insert(Increment(_maxA), val).a;
        }

        public T2 Get(T1 val) => _forward[val];
        public T1 Get(T2 val) => _reverse[val];
            
        public (T1 a,T2 b) Insert(T1 a, T2 b)
        {
            _forward[a] = b;
            _reverse[b] = a;
            if (a.CompareTo(_maxA) > 0) _maxA = a;
            return (a, b);
        }
    }
}
