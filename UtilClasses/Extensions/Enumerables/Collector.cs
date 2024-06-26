using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilClasses.Extensions.Enumerables
{
    public class Collector<TIn, TOut>
    {
        readonly List<TIn> _cols;
        List<TOut> _collected;
        public Collector(IEnumerable<TIn> cols)
        {
            _cols = cols.ToList();
            _collected = new List<TOut>();
        }

        public Collector<TIn, TOut> Collect(Func<TIn, TOut> f) => Do(_cols.Select(f));

        public Collector<TIn, TOut> Collect(Func<TIn, bool> predicate, Func<TIn, TOut> f) =>
            Do(_cols.Where(predicate).Select(f));
        public Collector<TIn, TOut> Collect(params TOut[] ps) => Do(ps);

        public Collector<TIn, TOut> Collect(Func<TIn, IEnumerable<TOut>> f) => Do(_cols.SelectMany(f));

        public Collector<TIn, TOut> Collect(Func<TIn, bool> predicate, Func<TIn, IEnumerable<TOut>> f) =>
            Do(_cols.Where(predicate).SelectMany(f));
        public List<TOut> ToList() => new List<TOut>(_collected);

        private Collector<TIn, TOut> Do(IEnumerable<TOut> items)
        {
            _collected.AddRange(items);
            return this;
        }

        public Collector<TIn, TOut> Add(TOut v) => Do(new[] { v });
        public Collector<TIn, TOut> Add(IEnumerable<TOut> vs) => Do(vs);
        public static implicit operator List<TOut>(Collector<TIn, TOut> c) => new List<TOut>(c._collected);
    }
}