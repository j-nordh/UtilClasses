using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace UtilClasses.Extensions.Lists
{
    public static class ListExtensions
    {
        public static void Add<T1, T2>(this List<Tuple<T1, T2>> list, T1 v1, T2 v2)
        {
            list.Add(new Tuple<T1, T2>(v1, v2));
        }

        public static void Shuffle<T>(this List<T> list)
        {
            var rnd = new Random();
            for (var i = 0; i < list.Count; ++i)
            {
                var i1 = rnd.Next(list.Count);
                var i2 = rnd.Next(list.Count);
                (list[i1], list[i2]) = (list[i2], list[i1]);
            }
        }

        public static void Add<T1, T2>(this List<KeyValuePair<T1, T2>> list, T1 v1, T2 v2)
        {
            list.Add(new KeyValuePair<T1, T2>(v1, v2));
        }

        public static void AddRange<T>(this List<T> lst, params T[] items)
        {
            lst.AddRange(items);
        }

        public static void AddRanges<T>(this List<T> lst, params IEnumerable<T>[] ranges)
        {
            foreach(var range in ranges)
                lst.AddRange(range);
        }
        public static void AddRange<T>(this List<T> lst, params (bool predicate, T value)[] tuples) =>
            lst.AddRange(tuples.Where(t => t.predicate).Select(t => t.value));
        public static void AddRange<T>(this List<T> lst, bool predicate, params T[] values)
        {
            if (!predicate)
                return;
            lst.AddRange(values);
        }

        public static void AddRange<T>(this List<T> lst, T obj, int repeat) => lst.AddRange(Enumerable.Repeat(obj, repeat));
        public static IEnumerable<T1> Keys<T1, T2>(this List<KeyValuePair<T1, T2>> lst) => lst.Select(i => i.Key);
        public static T AddAndReturn<T>(this List<T> lst, T val)
        {
            lst.Add(val);
            return val;
        }

        public static T FirstOrAdd<T>(this List<T> lst, Func<T, bool> predicate, Func<T> createFunc) where T : class =>
            lst.FirstOrDefault(predicate) ?? lst.AddAndReturn(createFunc());

        public static T FirstOrAdd<T>(this List<T> lst, Func<T, bool>? predicate = null) where T : class, new() =>
            (null == predicate
            ? lst.FirstOrDefault()
            : lst.FirstOrDefault(predicate)
            ) ?? lst.AddAndReturn(new T());

        public static List<T> Include<T>(this List<T> lst, IEnumerable<T> items)
        {
            lst.AddRange(items);
            return lst;
        }

        public static List<T> MaybeInclude<T>(this List<T> lst, bool include, Func<IEnumerable<T>> items)
        {
            if (!include) return lst;
            return lst.Include(items());
        }
        public static List<T> MaybeInclude<T>(this List<T> lst, bool include, T item)
        {
            if (!include) return lst;
            lst.Add(item);
            return lst;
        }

        public static List<T> MaybeInclude<T>(this List<T> lst, params (bool, T)[] args)
        {
            foreach (var (predicate, item) in args)
            {
                lst.MaybeInclude(predicate, item);
            }
            return lst;
        }
        public static void Add<T>(this List<T> lst, (bool predicate, T item) arg)
        {
            lst.MaybeInclude(arg);
        }
        public static void Add<T>(this List<T> lst, bool? predicate, T item) => lst.Add((predicate ?? false, item));
        public static void Add<T>(this List<T> lst, bool predicate, T item) => lst.Add((predicate, item));
        public static void Add<T>(this List<T> lst, (bool predicate, IEnumerable<T> items) arg)
        {
            lst.MaybeInclude(arg.predicate, () => arg.items);
        }


        public static int BinarySearch<TVal, TKey>(this List<TVal> lst, TKey key, Func<TVal, TKey, int> compare)
        {
            var floor = 0;
            var ceiling = lst.Count;
            var lastP = 0;
            while (true)
            {
                var p = floor + (ceiling - floor) / 2;
                if (lastP == p) return -(p + 1);
                lastP = p;
                var res = compare(lst[p], key);
                if (res == 0) return p;
                if (res < 0) ceiling = p;
                if (res > 0) floor = p;
            }
        }

        public static (int, TVal) BinaryGetOrAdd<TKey, TVal>(this List<TVal> lst, TKey key, Func<TVal, TKey, int> comparer, Func<TKey, TVal> constructor)
        {
            var i = lst.BinarySearch(key, comparer);
            if (i >= 0) return (i, lst[i]);
            var o = constructor(key);
            lst.Insert(~i, o);
            return (~i, o);
        }

        public static T GetOrAdd<T>(this List<T> lst, Func<T, bool> predicate, Func<T> creator) where T:class
        {
            var ret = lst.FirstOrDefault(predicate);
            if (null != ret) return ret;
            ret = creator();
            lst.Add(ret);
            return ret;
        }
        public static T? GetOrAdd<T>(this List<T?> lst, int i, Func<T> creator) where T : class
        {

            if (lst.Count > i)
                return lst[i];

            lst.EnsureLength(i + 1);

            var ret = creator();
            lst[i] = ret;
            return ret;
        }

        public static List<T?> EnsureLength<T>(this List<T?> lst, int length) where T : class
        {
            while (lst.Count < length)
                lst.Add(null);
            return lst;
        }


        public static T? GetOrAdd<T>(this List<T?> lst, int i) where T : class, new() => lst.GetOrAdd(i, () => new());

        public static T Maybe<T>(this List<T> lst, int i) where T : class => lst.Count > i? lst[i] : null;

        [Pure]
        public static int LastIndexOf<T>(this List<T> lst, Func<T, bool> predicate)
        {
            for (int i = lst.Count - 1; i >= 0; i--)
            {
                if (predicate(lst[i])) return i;
            }

            return -1;
        }

        public static List<T1> ZipInPlace<T1, T2>(this List<T1> lst1, IEnumerable<T2> items2,
            Func<T1, T2, T1> f)
        {
            var lst2 = items2.ToList();
            var end = new[] { lst1.Count, lst2.Count }.Min();
            for (int i = 0; i < end; i++)
            {
                lst1[i] = f(lst1[i], lst2[i]);
            }
            return lst1;
        }

        public static List<T1> ZipInPlace<T1, T2>(this List<T1> lst1, IEnumerable<T2> items2,
            Action<T1, T2> a) => lst1.ZipInPlace(items2, (o1, o2) =>
        {
            a(o1, o2);
            return o1;
        });

        public static List<T> Dequeue<T>(this List<T>lst, int count)
        {
            var ret = lst.Take(count).ToList();
            lst.RemoveRange(0, count);
            return ret;
        }

        public static List<T> SetContent<T>(this List<T> lst, IEnumerable<T> newContent)
        {
            lst.Clear();
            lst.AddRange(newContent);
            return lst;
        }

        public static List<T?> SetItem<T>(this List<T?> lst, int index, T val) where T : class
        {
            lst.EnsureLength(index + 1);
            lst[index] = val;
            return lst;
        }

        public static void PerformAndClear<T>(this List<T?> lst, Action<T> action)
        {
            foreach (var o in lst)
            {
                action(o);
            }
            lst.Clear();
        }
    }
}
