using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Tasks;
using UtilClasses.Interfaces;


namespace UtilClasses.Extensions.Enumerables
{
    public static class EnumerableExtensions
    {
        [ContractAnnotation("enumerable:null => true")]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
        {
            return null == enumerable || !enumerable.Any();
        }

        public static List<T> RepeatCall<T>(Func<T> f, int count)
        {
            var lst = new List<T>();
            for (int i = 0; i < count; i++)
            {
                lst.Add(f());
            }
            return lst;
        }

        public static async Task<int> Sum(this IEnumerable<Task<int>> items)
        {
            int sum =0 ;
            foreach (var i in items)
                sum += await i;
            return sum;
        }
        public static async Task<int> Sum<T>(this IEnumerable<T> items, Func<T,Task<int>> f)
        {
            int sum = 0;
            foreach (var i in items)
                sum += await f(i);
            return sum;
        }

        public static IEnumerable<int> Count()
        {
            int count = 0;
            while (count < int.MaxValue)
            {
                yield return count++;
            }
        }

        public static IEnumerable<int> Count(int elems)
        {
            return Count().Take(elems);
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> elems) => new Queue<T>(elems);
        public static IEnumerable<T> Dequeue<T>(this Queue<T> q, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!q.Any()) throw new IndexOutOfRangeException("Try to dequeue more elements than the queue contained");
                yield return q.Dequeue();
            }
        }

        public static IEnumerable<T> DequeueUpTo<T>(this Queue<T> q, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!q.Any()) yield break;
                yield return q.Dequeue();
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> elems, Func<T, bool> pred)
        {
            int count = 0;
            foreach (var e in elems)
            {
                if (pred(e)) return count;
                count += 1;
            }
            return -1;
        }
        public static int IndexOf<T>(this T[] elems, Func<T, bool> pred, int start)
        {
            for (int i = start; i < elems.Length; i++)
            {
                if (pred(elems[i]))
                    return i;
            }
            return -1;
        }

        public static T[] Subset<T>(this T[] arr, int start, int len)
        {
            var ret = new T[len];
            Array.Copy(arr, start, ret, 0, len);
            return ret;
        }

        public static IEnumerable<TRes> CombinatoryZip<T1, T2, TRes>(this IEnumerable<T1> elems1,
            IEnumerable<T2> elems2,
            Func<T1, T2, TRes> f)
        {
            return elems1.SelectMany(e => elems2, (e, e2) => f(e, e2));
        }

        public static IEnumerable<Tuple<T, T>> Combinations<T>(this IEnumerable<T> elems)
        {
            var lst = elems.ToList();
            return lst.SelectMany((e, i) => lst.Skip(i + 1).Select(e2 => new Tuple<T, T>(e, e2)));
        }

        public static int One<T>(this IEnumerable<T> elems) => 1;

        public static List<T> AsSorted<T>(this IEnumerable<T> elems, Comparison<T> f)
        {
            var list = new List<T>(elems);
            list.Sort(f);
            return list;
        }

        public static List<T> AsSorted<T>(this IEnumerable<T> elems) where T : IComparable<T>
        {
            var list = new List<T>(elems);
            list.Sort((a, b) => a.CompareTo(b));
            return list;
        }
        public static List<T> AsSorted<T, TKey>(this IEnumerable<T> elems, Func<T, TKey> f) where TKey : IComparable<TKey>
        {
            var list = new List<T>(elems);
            list.Sort((a, b) => f(a).CompareTo(f(b)));
            return list;
        }

        public static string AsString(this IEnumerable<char> chars) => new (chars.ToArray());
        public static string AsString(this char c) => new (new []{c});
        public static List<T> Reversed<T>(this IEnumerable<T> elems)
        {
            if (null == elems) return null;
            var ret = new List<T>(elems);
            ret.Reverse();
            return ret;
        }

        //public static IEnumerable<T> NotNull<T>(this IEnumerable<T> elems) where T : class =>
        //    elems.Where(e => e != null);
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> elems) where T : class =>
            elems.Where(e => e != null).Select(e=>e!);

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> elems) where T : struct =>
            elems.Where(e => e.HasValue).Select(e => e.Value);

        public static IEnumerable<(TKey, TVal)> NotNull<TKey, TVal>(this IEnumerable<(TKey? K, TVal V)> items)
            where TKey : struct => items.Where(t => t.K != null).Select(t => (t.K.Value, t.V));
        public static void Add<T1, T2>(this List<Tuple<T1, T2>> lst, T1 v1, T2 v2) =>
            lst.Add(new Tuple<T1, T2>(v1, v2));

        public static IEnumerable<T> Leave<T>(this IEnumerable<T> elems, int count)
        {
            var lst = elems.ToList();
            lst.Reverse();
            return lst.Skip(count).Reverse();
        }

        public static IEnumerable<string?> ToStrings<T>(this IEnumerable<T> items) => items.Select(i => i?.ToString());

        public static IEnumerable<Action> RunAll(this IEnumerable<Action> items) => items.ForEach(a => a(), ex => throw ex);
        public static List<T> ForEach<T>(this IEnumerable<T> items, Action<T> action) => items.ForEach(action, ex => throw ex);
        public static List<T> ForEach<T>(this IEnumerable<T> items, Action<T> action, Action<Exception> handler) => items.RunForEach(o=> { action(o); return 0; }, handler);

        public static List<T> ForEach<T, TOut>(this IEnumerable<T> items, Func<T, TOut> func) => items.RunForEach(func, ex => throw ex);
        public static List<T> ForEach<T, TOut>(this IEnumerable<T> items, Func<T, TOut> func, Action<Exception> handler) => items.RunForEach(func, handler);
        private static List<T> RunForEach<T, TOut>(this IEnumerable<T> items, Func<T, TOut> func, Action<Exception> handler)
        {
            Ensure.Argument.NotNull(items, "enumerable");
            Ensure.Argument.NotNull(func, "func");
            var ret = new List<T>();

            foreach (T value in items)
            {
                try
                {
                    func(value);
                    ret.Add(value);
                }
                catch (Exception ex)
                {
                    handler(ex);
                }
            }
            return ret;
        }

        public static List<T> Into<T>(this IEnumerable<T> items, List<T> target)
        {
            target.AddRange(items);
            return target;
        }

        public static T[][] ToMatrix<T>(this IEnumerable<IEnumerable<T>> values)
        {
            if (null == values) return null;
            var lst = values.Select(row => row.ToList()).ToList();
            if (lst.Count == 0) return new T[0][];
            var colCount = lst.First().Count();
            if (lst.Any(r => r.Count != colCount)) throw new ArgumentException("Not all rows have equal length");
            var ret = new T[lst.Count][];
            lst.ForEach((i, r) => ret[i] = r.ToArray());
            return ret;
        }

        public static T2 Aggregate<T, T2>(this IEnumerable<T> items, T2 seed, Action<T2, T> action) => items.Aggregate(
            seed, (s, v) =>
            {
                action(s, v);
                return s;
            });

        public static async Task<IEnumerable<T>> ForEachAsync<T>(this Task<IEnumerable<T>> enumerable, Func<T, Task> action)
            => await (await enumerable).ForEachAsync(action);

        public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            Ensure.Argument.NotNull(enumerable, "enumerable");
            Ensure.Argument.NotNull(action, "action");
            await enumerable.Select(action).WhenAll_Throw();
            return enumerable;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<int, T> action)
        {
            int i = 0;
            foreach (T o in enumerable)
            {
                action(i, o);
                i += 1;
            }
            return enumerable;
        }

        /// <summary>
        /// Convenience method for retrieving a specific page of items within a collection.
        /// </summary>
        /// <param name="source">The enumerable to paginate</param>
        /// <param name="pageIndex">The index of the page to get.</param>
        /// <param name="pageSize">The size of the pages.</param>
        public static IEnumerable<T> GetPage<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            Ensure.Argument.NotNull(source, "source");
            Ensure.Argument.Is(pageIndex >= 0, nameof(pageIndex), "The page index cannot be negative.");
            Ensure.Argument.Is(pageSize > 0, nameof(pageSize), "The page size must be greater than zero.");

            return source.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public static void Paginate<T>(this IEnumerable<T> source, int pageSize, Action<IEnumerable<T>> a) => source.Paginate(pageSize).ForEach(a);
        public static IEnumerable<IEnumerable<T2>> Paginate<T,T2>(this IEnumerable<T> source, int pageSize, Func<IEnumerable<T>, IEnumerable<T2>> f) => source.Paginate(pageSize).Select(f);
        public static IEnumerable<IEnumerable<T>> Paginate<T>(this IEnumerable<T> source, int pageSize)
        {
            var q = source.ToQueue();
            while (q.Any())
            {
                yield return q.DequeueUpTo(pageSize);
            }
        }
        public static IEnumerable<T> Union<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(x => x);

        /// <summary>
        /// Converts an enumerable into a readonly collection
        /// </summary>
        public static IEnumerable<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable.ToList());
        }

        public static IEnumerable<TOut> SelectManyFuncs<TIn, TOut>(this IEnumerable<TIn> items, params Func<TIn, TOut>[] funcs) => items.SelectMany(i => funcs.Select(f => f(i)));

        /// <summary>
        /// Validates that the <paramref name="enumerable"/> is not null and contains items.
        /// </summary>
        [ContractAnnotation("enumerable:null=>false")]
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? enumerable)
        {
            return enumerable != null && enumerable.Any();
        }

        /// <summary>
        /// Concatenates the members of a collection, using the specified separator between each member.
        /// </summary>
        /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string. If values has no members, the method returns null.</returns>
        public static string JoinOrDefault<T>(this IEnumerable<T> values, string separator)
        {
            Ensure.Argument.NotNullOrEmpty(separator, "separator");

            if (values == null)
                return default(string);

            return string.Join(separator, values);
        }

        public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> items) =>
            items.SelectMany(lst => lst);


        public static IEnumerable<string> NotNullOrWhitespace(this IEnumerable<string?>? values) =>
            null == values
                ? new string[]{} 
                : values.Where(s => !s.IsNullOrWhitespace()).NotNull();

        public static bool ContainsOic(this IEnumerable<string>? hay, string? needle)
        {
            var lst = hay?.ToList();
            if (lst.IsNullOrEmpty()) return false;
            return null == needle 
                ? lst.Any(o => o == null) 
                : lst.Contains(needle, StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<T> Update<T>(this IEnumerable<T> items, Func<T, bool> predicate, Action<T> updater)
        {
            if (null == items) return null;
            var lst = items.ToList();
            foreach (var item in lst.Where(predicate))
            {
                updater(item);
            }
            return lst;
        }

        public static decimal Median(this IEnumerable<decimal> items) => items.Median(x => x);

        public static decimal Median<T>(this IEnumerable<T> items, Func<T, decimal> f)
        {
            var lst = items.Select(f).AsSorted();
            return lst.Count % 2 == 0
                ? lst.Skip(lst.Count / 2 - 1).Take(2).Average()
                : lst[lst.Count / 2];
        }

        public static T Median<T>(this IEnumerable<T> items) where T : IComparable<T> => Median(items, x => x);

        public static T Median<T>(this IEnumerable<T> items, Func<T, T> f) where T:IComparable<T>
        {
            if (items.Count() % 2 == 0) throw new ArgumentException("Items must be odd");

            var lst = items.Select(f).AsSorted();
            return lst[lst.Count / 2];
        }

        public static T Median<T>(this IEnumerable<T> items, Func<T, T> f, Func<IEnumerable<T>, T> t) where T : IComparable<T>
        {
            var lst = items.Select(f).AsSorted();
            return lst.Count % 2 == 0
                ? t(lst.Skip(lst.Count / 2 - 1).Take(2))
                : lst[lst.Count / 2];
        }

        public static HashSet<T> AddRange<T>(this HashSet<T> set, IEnumerable<T> items) =>
            set?.Do(s => items.ForEach(i => s.Add(i)));

        public static IEnumerable<T> Apply<T>(this IEnumerable<T> items, Action<T> a)
        {
            foreach (var item in items)
            {
                a(item);
                yield return item;
            }
        }

        public static IEnumerable<IEnumerable<TOut>> Split<T, TOut>(this IEnumerable<T> items, params (Func<T, bool> predicate, Func<T, TOut> f)[] paths)
        {
            foreach (var path in paths)
            {
                yield return items.Where(path.predicate).Select(path.f);
            }
        }

        public static List<List<TOut>> Split<T, TOut>(this IEnumerable<T> items, int index, params Func<IEnumerable<T>, IEnumerable<TOut>>[] fs)
        {
            if (fs.Count() != 2) throw new ArgumentException("fs must contain exactly 2 functions.");
            var count = 0;
            var q = items.ToQueue();
            var ret = new List<List<TOut>>();
            foreach(var f in fs)
            {
                if (!q.Any()) break;
                if (count >= fs.Count()) break;
                ret.Add(f(q.Dequeue(index)).SmartToList());
            }
            return ret;
        }
        public static void Split<T>(this IEnumerable<T> items, int index, params Action<IEnumerable<T>>[] actions)
        {
            items.Split(index, actions.Select<Action<IEnumerable<T>>, Func<IEnumerable<T>, IEnumerable <int>>>(a => x => { a(x); return new[] { 1 }; }).ToArray());
        }

        public static IEnumerable<TOut> Split<TIn, TDiscriminator, TOut>(this IEnumerable<TIn> items, Func<TIn, TDiscriminator> selector, params (Func<TDiscriminator, bool> predicate, Func<TIn, TOut> f)[] paths)
        {
            foreach (var i in items)
            {
                var v = selector(i);
                foreach (var p in paths)
                {
                    if (p.predicate(v)) yield return p.f(i);
                }
            }
        }

        public static IEnumerable<TOut> Split<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, long> selector, params (long val, Func<TIn, TOut> f)[] paths) =>
            items.Split(selector, paths
                .Select<(long val, Func<TIn, TOut> f), (Func<long, bool>, Func<TIn, TOut>)>(p => (v => v == p.val, p.f))
                .ToArray());

        public static IEnumerable<TIn> Split<TIn>(this IEnumerable<TIn> items, Func<TIn, long> selector, params (long val, Action<TIn> f)[] paths) =>
            items.Split(selector, paths.Select(t => new ActionPath<TIn, long>(t)));
        class ActionPath<TIn, TDiscriminator>
        {
            public ActionPath(Func<TDiscriminator, bool> predicate, Action<TIn> action)
            {
                Predicate = predicate;
                Action = action;
            }
            public ActionPath((Func<TDiscriminator, bool> predicate, Action<TIn> a) t) : this(t.predicate, t.a)
            { }
            public ActionPath((TDiscriminator val, Action<TIn> a) t) : this(v => v!.Equals(t.val), t.a)
            { }
            public Func<TDiscriminator, bool> Predicate { get; }
            public Action<TIn> Action { get; }
        }

        class FuncPath<TIn, TDiscriminator, TOut>
        {
            public FuncPath(Func<TDiscriminator, bool> predicate, Func<TIn, TOut> func)
            {
                Predicate = predicate;
                Func = func;
            }

            public FuncPath((Func<TDiscriminator, bool> predicate, Func<TIn, TOut> f) t) : this(t.predicate, t.f)
            { }

            public FuncPath((TDiscriminator val, Func<TIn, TOut> f) t) : this(v => v!.Equals(t.val), t.f)
            { }


            public Func<TDiscriminator, bool> Predicate { get; }
            public Func<TIn, TOut> Func { get; }
        }


        private static IEnumerable<TIn> Split<TIn, TDiscriminator>(this IEnumerable<TIn> items, Func<TIn, TDiscriminator> selector, IEnumerable<ActionPath<TIn, TDiscriminator>> paths)
        {
            foreach (var i in items)
            {
                var v = selector(i);
                foreach (var p in paths)
                {
                    if (p.Predicate(v)) p.Action(i);
                }

                yield return i;
            }
        }

        public static IEnumerable<T> Join<T>(this IEnumerable<IEnumerable<T>> enumerables)
        {
            foreach (var e in enumerables)
            {
                foreach (var item in e)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> items, ISet<T> set) =>
            items.Where(i => i != null && set.Contains(i));

        public static IEnumerable<TOut> SplitTransformJoin<T, TOut>(this IEnumerable<T> items, params (Func<T, bool> predicate, Func<T, TOut> f)[] paths) => items.Split(paths).Join();
        public static IEnumerable<TOut> SplitTransformJoin<T, TOut>(this IEnumerable<T> items, params Func<T, TOut>[] paths) => items.Split(paths.Select<Func<T, TOut>, (Func<T, bool> predicate, Func<T, TOut> f)>(p=> (_=>true, p)).ToArray()).Join();

        public static IEnumerable<TOut> SelectManyStripNull<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, IEnumerable<TOut>> f) where TIn : class where TOut : class
            => items.NotNull().Select(f).NotNull().SelectMany().NotNull();

        [ContractAnnotation("allowNull:false=>notnull")]
        [ContractAnnotation("allowNull:true=>canbenull")]
        public static List<T>? SmartToList<T>(this IEnumerable<T>? items, bool allowNull = false)
        {
            if (null == items) return allowNull ? null : new List<T>();
            var lst = items.ToList();
            if (!lst.Any()) return new List<T>();
            return items as List<T> ?? lst?.ToList() ?? (allowNull ? null : new List<T>());
        }

        //public static IEnumerable<TOut> Select<TIn, TOut>(this IEnumerable<TIn> items, Dictionary<TIn, TOut> dict)
        //{
        //    foreach (var i in items)
        //    {
        //        yield return dict[i];
        //    }
        //}
        //public static TOut Sum<TIn, TOut>(this IEnumerable<TIn> items, Dictionary<TIn, TOut> dict) => items.Aggregate(dict, (a,b)=>Operator)
        public static TOut Aggregate<TIn, TOut>(this IEnumerable<TIn> items, Dictionary<TIn, TOut> dict,Func<TOut, TOut, TOut> f)
        {
            return items.Where(dict.ContainsKey).Select(k =>dict[k]).Aggregate(f);
        }

        public static T FirstOrThrow<T>(this IEnumerable<T> items, Func<T, bool> predicate, Func<Exception> exCreator) where T : class
        {
            var ret = items.FirstOrDefault(predicate);
            if (null != ret) return ret;
            throw exCreator();
        }

        public static T FirstWithGuid<T>(this IEnumerable<T> items, Guid id) where T : IHasGuid =>
            items.FirstOrDefault(i => i.Id == id);

        public static T FirstOrDefault<T>(this IEnumerable<T> items, Func<T, bool> predicate, T defaultValue)
        {
            var filtered = items.Where(predicate);
            return filtered.Any() ? filtered.First() : defaultValue;
        }

        public static T FirstTrue<T>(this IEnumerable<(bool, T)> items, T defaultValue) =>
            items.FirstOrDefault(x => x.Item1, (false, defaultValue)).Item2;

        public static T FirstOrDefault<T>(this IEnumerable<T> items, T defaultValue) => items.Any() ? items.First() : defaultValue;

        public static T? FirstOrNull<T>(this IEnumerable<T> items) where T : struct =>
            items.FirstOrNull(_ => true);

        public static T? FirstOrNull<T>(this IEnumerable<T> items, Func<T, bool> predicate) where T : struct
        {
            var lst = items.Where(predicate).ToList();
            return lst.Any() ? (T?)lst.First() : null;
        }

        public static List<T> Intersect<T>(this IEnumerable<IEnumerable<T>> bunches, IEqualityComparer<T> cmp)
        {
            if (null == bunches) return null;
            var lists = bunches.Select(b => b.ToList()).ToList();
            if (lists.Count == 1) return lists.Single();
            var ret = lists.First();
            foreach (var lst in lists.Skip(1))
                ret = ret.Intersect(lst, cmp).ToList();

            return ret;
        }

        public static async Task<IEnumerable<TRes>> ZipAsync<T1, T2, TRes>(this Task<IEnumerable<T1>> first,
            IEnumerable<T2> second, Func<T1, T2, TRes> f) => (await first).Zip(second, f);
        public static async Task<IEnumerable<TRes>> ZipAsync<T1, T2, TRes>(this Task<List<T1>> first,
            IEnumerable<T2> second, Func<T1, T2, TRes> f) => (await first).Zip(second, f);

        public static bool IsGrowing<T>(this IEnumerable<T> items) where T : IComparable
        {
            var lst = items.ToList();
            var prev = lst.First();
            foreach (var i in lst.Skip(1))
            {
                if (i.CompareTo(prev) <= 0) return false;
                prev = i;
            }

            return true;
        }

        public static bool All<T>(this IEnumerable<Func<T, bool>> funcs, T parameter) => funcs.Select(f => f(parameter)).All(b => b);
        public static T MaxOr<T>(this IEnumerable<T> items, T defaultValue) => items.IsNullOrEmpty() ? defaultValue : items.Max();

        public static DictBuilder<T, TKey> BuildDictionary<T, TKey>(this IEnumerable<T> items) => new(items);
        public static DictBuilder<T, TKey, TVal> BuildDictionary<T, TKey, TVal>(this IEnumerable<T> items, Func<T, TVal> valFunc) => new(items, valFunc);
        public static DictBuilder<T, string, string> BuildDictionary<T>(this IEnumerable<T> items, Func<T, string> valFunc, StringComparer cmp) => new DictBuilder<T, string, string>(items, valFunc).WithComparer(cmp);

        public static Collector<TIn, string> GetStringCollector<TIn>(this IEnumerable<TIn> items) => new Collector<TIn, string>(items);
        public static Collector<TIn, TOut> Collect<TIn, TOut>(this IEnumerable<TIn> items) => new Collector<TIn, TOut>(items);
        public static Collector<TIn, TOut> Collect<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> f)
        {
            var coll = new Collector<TIn, TOut>(items);
            coll.Collect(f);
            return coll;
        }
        public static Collector<TIn, TOut> Collect<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, IEnumerable<TOut>> f)
        {
            var coll = new Collector<TIn, TOut>(items);
            coll.Collect(f);
            return coll;
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> items, params Expression<Func<T, object>>[] fs) => items.Distinct(new GenericEqualityComparer<T>(fs));

        public static IEnumerable<T> Except<T, TSet>(this IEnumerable<T> items, TSet filter) where TSet:ISet<T> =>
            items.Where(i => !filter.Contains(i));
        public static IEnumerable<T> Except<T, T2>(this IEnumerable<T> items, IDictionary<T, T2>  filter)=>
            items.Where(i => !filter.ContainsKey(i));
    }

    public static class Enumerable<T> where T : IComparable
    {
        public static IEnumerable<T> Range(T start, T end, Func<T, T> increment)
        {
            var x = start;
            while (x.CompareTo(end) < 0)
            {
                yield return x;
                x = increment(x);
            }
        }
    }




}
