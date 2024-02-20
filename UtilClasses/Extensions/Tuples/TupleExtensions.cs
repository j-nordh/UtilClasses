using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace UtilClasses.Extensions.Tuples
{
    public static class TupleExtensions
    {
        public static IEnumerable<T> AsEnumberable<T>(this (T, T) t) => new[] { t.Item1, t.Item2 };
        public static (T,T) AsTuple2<T>(this IEnumerable<T> items) => (items.First(), items.Skip(1).First());
    }
}
