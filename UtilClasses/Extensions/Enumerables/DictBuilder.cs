using System;
using System.Collections.Generic;
using UtilClasses.Extensions.Objects;

namespace UtilClasses.Extensions.Enumerables
{
    public class DictBuilder<T, TKey> : DictBuilder<T, TKey, T>
    {
        public DictBuilder(IEnumerable<T> objects) : base(objects, x=>x)
        {
        }
    }
    public class DictBuilder<T, TKey, TVal>
    {
        private readonly IEnumerable<T> _objects;
        private readonly Func<T, TVal> _valFunc;
        private List<Func<T, TKey>> _keyFuncs = new List<Func<T, TKey>>();
        private IEqualityComparer<TKey> _cmp;

        public DictBuilder(IEnumerable<T> objects, Func<T, TVal> valFunc)
        {
            _objects = objects;
            _valFunc = valFunc;
        }

        public DictBuilder<T, TKey, TVal> WithKeyFunc(Func<T, TKey> f)
        {
            _keyFuncs.Add(f);
            return this;
        }

        public DictBuilder<T, TKey, TVal> WithComparer(IEqualityComparer<TKey> cmp) => this.Do(() => _cmp = cmp);

        public Dictionary<TKey, TVal> Build()
        {
            var dict = new Dictionary<TKey, TVal>();
            foreach (var o in _objects)
            {
                foreach (var kf in _keyFuncs)
                {
                    dict[kf(o)] = _valFunc(o);
                }
            }
            return dict;
        }

    }
}