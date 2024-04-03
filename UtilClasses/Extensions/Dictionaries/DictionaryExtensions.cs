using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;
using UtilClasses.Interfaces;

namespace UtilClasses.Extensions.Dictionaries
{
    public static class DictionaryExtensions
    {
        public static bool IsSetAnd<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, Func<TVal, bool> f)
        {
            return dict.ContainsKey(key) && f(dict[key]);
        }

        public static bool IsSet_GTZ<TKey>(this Dictionary<TKey, int> dict, TKey key)
        {
            return IsSetAnd(dict, key, x => x > 0);
        }

        [Obsolete("Use ToDictionaryOic instead.")]
        public static DictionaryOic<TElement> ToCaseInsensitiveDictionary<TSource, TElement>(
            this IEnumerable<TSource> source, Func<TSource, string> keySelector,
            Func<TSource, TElement> elementSelector) where TElement : class
            => ToDictionaryOic(source, keySelector, elementSelector);

        public static DictionaryOic<TElement> ToDictionaryOic<TSource, TElement>(
            this IEnumerable<TSource> source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector)
        {
            var dict = new DictionaryOic<TElement>();
            foreach (var item in source)
            {
                dict[keySelector(item)] = elementSelector(item);
            }
            return dict;
        }
        [Obsolete("Use ToDictionaryOic instead.")]
        public static DictionaryOic<T> ToCaseInsensitiveDictionary<T>(
            this IEnumerable<T> source, Func<T, string> keySelector) where T : class =>
            source.ToDictionaryOic(keySelector, x => x);

        public static DictionaryOic<T> ToDictionaryOic<T>(
            this IEnumerable<KeyValuePair<string, T>> source) =>
            source.ToDictionaryOic(kvp => kvp.Key, kvp => kvp.Value);
        public static DictionaryOic<T> ToDictionaryOic<T>(
            this IEnumerable<T> source, Func<T, string> keySelector) where T : class =>
            source.ToDictionaryOic(keySelector, x => x);

        [Obsolete("Use ToDictionaryOic instead.")]
        public static DictionaryOic<TElement> ToCaseInsensitiveDictionary<TElement>(
            this IDictionary<string, TElement> source) where TElement : class
            => source.ToDictionaryOic();
        public static DictionaryOic<TElement> ToDictionaryOic<TElement>(
            this IDictionary<string, TElement> source) where TElement : class
        {
            var dict = new DictionaryOic<TElement>();
            foreach (var key in source.Keys)
            {
                dict[key] = source[key];
            }
            return dict;
        }

        public static bool MaybeAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue? val) where TValue : struct
        {
            if (!val.HasValue) return false;
            dict.Add(key, val.Value);
            return true;
        }


        public static Dictionary<TKey, TValue> SupplementedWith<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            Dictionary<TKey, TValue> supplement)
        {
            var ret = new Dictionary<TKey, TValue>(supplement);
            foreach (var kvp in dict)
            {
                ret[kvp.Key] = kvp.Value;
            }
            return ret;
        }

        public static TValue? Maybe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Action? onNull = null, RequireClass<TValue>? _ = null) where TValue : class
        {
            if (null != dict)
            {
                if (dict.TryGetValue(key, out var ret)) return ret;
            }
            onNull?.Invoke();
            return null;
        }
        public static TValue? Maybe<TKey, TValue>(this IDictionary<TKey, TValue>? dict, TKey? key, Action? onNull = null, RequireClass<TValue>? _ = null) where TValue : class where TKey : struct 
        {
            if (null != key && null != dict)
            {
                if (dict.TryGetValue(key.Value, out var ret)) return ret;
            }
            onNull?.Invoke();
            return null;
        }

        public static bool IsTrue<TKey>(this IDictionary<TKey, string> dict, TKey key) => dict.Maybe(key).AsBoolean();

        public static TValue? Maybe<TKey, TValue>(this IDictionary<TKey, TValue?> dict, TKey key, Action? onNull = null) where TValue : struct
        {
            if (dict.TryGetValue(key, out TValue? ret)) return ret;
            onNull?.Invoke();
            return null;
        }

        public class RequireStruct<T> where T : struct { }
        public class RequireClass<T> where T : class { }
        public static TValue? Maybe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Action? onNull = null, RequireStruct<TValue>? _ = null) where TValue : struct
        {
            if (dict.TryGetValue(key, out var val)) return val;
            onNull?.Invoke();
            return null;
        }

        public static TValue Maybe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue def)
        {
            TValue ret;
            if (null == dict) return def;
            return dict.TryGetValue(key, out ret) ? ret : def;
        }
        public static TValue Maybe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey? key, TValue def) where TKey: struct
        {
            TValue ret;
            if (null == key) return def;
            if (null == dict) return def;
            return dict.TryGetValue(key.Value, out ret) ? ret : def;
        }

        public static bool AsBoolean<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : class
        {
            var obj = dict.Maybe(key);
            return (obj as string ?? obj?.ToString()).AsBoolean();
        }

        public static Dictionary<TKey, TVal> Set<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val)
        {
            dict[key] = val;
            return dict;
        }

        public static Dictionary<TKey, TVal> Set<TKey, TVal>(this Dictionary<TKey, TVal> dict,
            IEnumerable<Tuple<TKey, TVal>> tuples)
        {
            foreach (var t in tuples)
            {
                dict[t.Item1] = t.Item2;
            }
            return dict;
        }

        public static TVal GetOrAdd<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, Func<TVal> addFunc)
        {
            TVal res;
            if (dict.TryGetValue(key, out res)) return res;
            res = addFunc();
            dict[key] = res;
            return res;
        }

        public static TVal GetOrThrow<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, Action thrower)
        {
           TVal res;
            if (dict.TryGetValue(key, out res)) return res;
            thrower();
            return res;
        }

        public static TVal GetOrAdd<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, Func<TKey, TVal> addFunc)
        {
            TVal res;
            if (dict.TryGetValue(key, out res)) return res;
            res = addFunc(key);
            dict[key] = res;
            return res;
        }
        public static async Task<TVal> GetOrAdd_Async<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, Func<TKey, Task<TVal>> addFunc)
        {
            TVal res;
            if (dict.TryGetValue(key, out res)) return res;
            res = await addFunc(key);
            dict[key] = res;
            return res;
        }

        public static TVal GetOrAdd<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, TVal defaultValue) => dict.GetOrAdd(key, () => defaultValue);

        public static IDictionary<TKey2, TVal> GetOrAdd<TKey, TKey2, TVal>(this IDictionary<TKey, IDictionary<TKey2, TVal>> dict, TKey key,
            IEqualityComparer<TKey2> cmp)
            => dict.GetOrAdd(key, () => new Dictionary<TKey2, TVal>(cmp));

        public static async Task<TVal> GetOrAdd<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key,
            Func<Task<TVal>> addFunc)
        {
            TVal res;
            if (dict.TryGetValue(key, out res)) return res;
            res = await addFunc();
            dict[key] = res;
            return res;
        }

        public static TVal GetOrAdd<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key) where TVal : new() =>
            dict.GetOrAdd(key, () => new TVal());
        public static IDictionary<TKey, TVal> Add<TKey, TVal>(this IDictionary<TKey, TVal> dict, (TKey, TVal) t)
        {
            dict.Add(t.Item1, t.Item2);
            return dict;
        }

        public static TVal TryGetValue<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key) where TVal : class
        {
            TVal val;
            return dict.TryGetValue(key, out val) ? val : null;
        }

        public static TVal? TryGetValueStruct<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key) where TVal : struct
        {
            TVal val;
            return dict.TryGetValue(key, out val) ? val : (TVal?)null;
        }

        public static TVal TryGetValue<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, Func<Exception> onNull)
        {
            TVal val;
            if (dict.TryGetValue(key, out val)) return val;
            throw onNull();
        }

        public static Dictionary<T1, T2> UnionDict<T1, T2>(this Dictionary<T1, T2> dict1, Dictionary<T1, T2> dict2) =>
            dict1.AsEnumerable().Union(dict2.AsEnumerable()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public static Dictionary<string, T2> OverwritingToDictionary<T2, T3>(this IEnumerable<T3> items,
            Func<T3, string> keySelector, Func<T3, T2> valueSelector, StringComparer? cmp = null)
        {
            cmp ??= StringComparer.Ordinal;
            var dict = new Dictionary<string, T2>(cmp);
            foreach (var itm in items)
            {
                dict[keySelector(itm)] = valueSelector(itm);
            }
            return dict;
        }

        public static Dictionary<string, T2> ToDictionary<T, T2>(this IEnumerable<T> items, Func<T, string> keyFunc,
            Func<T, T2> valFunc, StringComparison sc)
        {
            var dict = new Dictionary<string, T2>(sc.ToStringComparer());
            foreach (var item in items)
            {
                dict[keyFunc(item)] = valFunc(item);
            }
            return dict;
        }

        public static Dictionary<Guid, T> ToDictionary<T>(this IEnumerable<T> items) where T : IHasGuid =>
            items.ToDictionary(i => i.Id, i => i);
        public static Dictionary<long, T> ToIdDictionary<T>(this IEnumerable<T> items) where T : IHasLongId =>
            items.ToDictionary(i => i.Id, i => i);
        public static List<KeyValuePair<T1, T2>> SnapshotList<T1, T2>(this Dictionary<T1, T2> dict)
        {
            //to preserve the respons as a snapshot, ie break the reference to the live dictionary.
            return dict.Select(kvp => new KeyValuePair<T1, T2>(kvp.Key, kvp.Value)).ToList();
        }

        public static IEnumerable<KeyValuePair<string?, string?>> ToStringStringKVPs<T1, T2>(
            this IEnumerable<KeyValuePair<T1, T2>> kvps) => kvps.Select(kvp => new KeyValuePair<string?, string?>(kvp.Key?.ToString(), kvp.Value?.ToString()));

        private static T2 DoUpdateValue<T1, T2>(this Dictionary<T1, T2> dict, T1 key, Func<T2, T2> updater,
            T2 defaultValue)
        {
            T2 val = dict.GetOrAdd(key, defaultValue);
            if (!dict.TryGetValue(key, out val)) val = defaultValue;
            val = updater(val);
            dict[key] = val;
            return val;
        }

        public static int Increment<T>(this Dictionary<T, int> dict, T key, int increment = 1, int defaultValue = 0) =>
            dict.DoUpdateValue(key, v => v + increment, defaultValue);
        public static long Increment<T>(this Dictionary<T, long> dict, T key, long increment = 1, long defaultValue = 0) =>
            dict.DoUpdateValue(key, v => v + increment, defaultValue);
        public static IDictionary<TKey, TVal> RemoveValue<TKey, TVal>(this IDictionary<TKey, TVal> dict, TVal val,
            IEqualityComparer<TVal> comp)
        {
            var keys = dict.Where(kvp => comp.Equals(kvp.Value, val)).Select(kvp => kvp.Key).ToList();
            foreach (var key in keys)
            {
                dict.Remove(key);
            }
            return dict;
        }

        public static IEnumerable<TVal> Select<TKey, TVal>(this IEnumerable<TKey> keys, Dictionary<TKey, TVal> dict) =>
            keys.Select(k => dict[k]);

        public static IDictionary<TKey, string> RemoveValue<TKey>(this IDictionary<TKey, string> dict, string val,
            StringComparison comp = StringComparison.OrdinalIgnoreCase) =>
            dict.RemoveValue(val, comp.ToEqualityComparer());

        public static void Add<TKey, TVal>(this Dictionary<TKey, TVal> dict, Tuple<TKey, TVal> v) =>
            dict[v.Item1] = v.Item2;

        public static TKey AddRetKey<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val)
        {
            dict[key] = val;
            return key;
        }
        //public static Guid Add<TVal>(this Dictionary<Guid, TVal> dict, TVal val)
        //{
        //    var key = Guid.NewGuid();
        //    dict[key] = val;
        //    return key;
        //}

        public static Guid Add<TVal>(this Dictionary<Guid, TVal> dict, TVal val) where TVal : IHasGuid
        {
            dict.Add(val.Id, val);
            return val.Id;
        }

        public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> items) => items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<(TKey, TVal)> items) => items.ToDictionary(t => t.Item1, t => t.Item2);
        public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> items, IEqualityComparer<TKey> comp) => items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, comp);


        public static Dictionary<TKey, TVal> Clone<TKey, TVal>(this Dictionary<TKey, TVal>? dict, Func<TKey, TKey>? keyCloner = null, Func<TVal, TVal>? valCloner = null)
        {
            if (null == dict) return null;
            keyCloner ??= (k => k);
            valCloner ??= (v => v);
            return dict.ToDictionary(kvp => keyCloner(kvp.Key), kvp => valCloner(kvp.Value));

        }

        public static IDictionary<TKey, TVal> IntoDict<TKey, TVal, TIn>(
            this IEnumerable<TIn> items,
            IDictionary<TKey, TVal>? dict,
            Func<TIn, TKey> keyFunc,
            Func<TIn, TVal> valFunc)
        {
            dict ??= new Dictionary<TKey, TVal>();
            foreach (var i in items)
                dict[keyFunc(i)] = valFunc(i);
            return dict;
        }

        public static IDictionary<string, TVal> PrefixKeys<TVal>(this Dictionary<string, TVal> dict, string prefix)
        {
            if (prefix.IsNullOrEmpty()) return dict;
            var keys = dict.Keys.ToList();
            foreach (var key in keys)
            {
                try
                {
                    dict[$"{prefix}{key}"] = dict[key];
                    dict.Remove(key);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            return dict;
        }

        public static IDictionary<string, TVal> PrefixIntoDict<TVal>(this Dictionary<string, TVal> items, string prefix,
            IDictionary<string, TVal> dict, char? separator = null)
        {
            string GetKey(KeyValuePair<string, TVal> kvp)
            {
                if (null == separator) return $"{prefix}{kvp.Key}";
                if (kvp.Key.IsNullOrEmpty()) return prefix.Trim(separator.Value);
                var newSeparator = $"{separator}";
                if (kvp.Key.All(c => c == '#')) newSeparator = "";
                return $"{prefix.Trim(separator.Value)}{newSeparator}{kvp.Key}";
            }

            return items.IntoDict(dict, GetKey, kvp => kvp.Value);
        }

        public static IDictionary<TKey, TVal> IntoDict<TKey, TVal>(this Dictionary<TKey, TVal> items,
            IDictionary<TKey, TVal> dict)
            => items.IntoDict(dict, kvp => kvp.Key, kvp => kvp.Value);

        public static IDictionary<TKey, TIn> IntoDict<TKey, TIn>(
            this IEnumerable<TIn> items,
            IDictionary<TKey, TIn> dict,
            Func<TIn, TKey> keyFunc) => items.IntoDict(dict, keyFunc, o => o);

        public static IDictionary<TKey, TVal> IntoDict<TKey, TVal>(
            this IDictionary<TKey, TVal> items,
            IDictionary<TKey, TVal> dict)
            => items.IntoDict(dict, k => k.Key, k => k.Value);

        public static IDictionary<TKey, TVal?> IntoDict<TKey, TVal>(
            this IDictionary<TKey, TVal> items,
            IDictionary<TKey, TVal?> dict) where TVal : struct
            => items.IntoDict(dict, k => k.Key, k => k.Value);
        public static TDict IntoDict<TKey, TVal, TDict>(this IEnumerable<(TKey Key, TVal Val)> items,
            TDict dict) where TDict : IDictionary<TKey, TVal>, new()
        {
            dict ??= new TDict();
            foreach (var i in items)
                dict[i.Key] = i.Val;
            return dict;
        }

        public static IDictionary<TKey, List<TVal>> IntoDict<TKey, TVal>(this IEnumerable<(TKey key, IEnumerable<TVal> vals)> ts,
            IDictionary<TKey, List<TVal>> dict)
        {
            dict ??= new Dictionary<TKey, List<TVal>>();
            foreach (var t in ts)
            {
                var (key, vals) = t;
                var lst = dict.GetOrAdd(key);
                lst.AddRange(vals);
            }
            return dict;
        }

        public static IDictionary<TKey, List<TVal>> IntoDict<TKey, TVal>(this (TKey key, IEnumerable<TVal> vals) t,
            IDictionary<TKey, List<TVal>> dict)
            => IntoDict(new[] { t }, dict);

        public static Dictionary<TKey, List<TVal>> Filter<TKey, TVal>(this Dictionary<TKey, List<TVal>> dict, TKey key,
            Func<TVal, bool> predicate)
        {
            if (dict == null) return null;
            if (!dict.ContainsKey(key)) return dict;
            if (dict[key].IsNullOrEmpty()) return dict;
            dict[key] = dict[key].Where(predicate).ToList();
            return dict;
        }

        //{
        //    dict = dict ?? new Dictionary<TKey, List<TVal>>();
        //    var (key, val) = t;
        //    var lst = dict.GetOrAdd(key);
        //    lst.Add(val);
        //    return dict;
        //}

    }
}
