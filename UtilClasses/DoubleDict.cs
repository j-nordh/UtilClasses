#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Dictionaries;

namespace UtilClasses;

public class DoubleDict<T1, T2> : IDoubleDict<T1, T2> where T1 : IComparable
{
    private Dictionary<T1, T2> _forward;
    private Dictionary<T2, T1> _reverse;

    public List<(T1 a, T2 b)> State => _forward.Select(kvp => (kvp.Key, kvp.Value)).ToList();

    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }
    public DoubleDict(IEqualityComparer<T1>? cmpA=null, IEqualityComparer<T2>? cmpB=null)
    {
        _forward = cmpA == null
            ? new Dictionary<T1, T2>()
            : new Dictionary<T1, T2>(cmpA);
        _reverse = cmpB == null
            ? new Dictionary<T2, T1>()
            : new Dictionary<T2, T1>(cmpB);
    }
    public DoubleDict(IEnumerable<(T1, T2)> items, IEqualityComparer<T1>? cmpA = null,
        IEqualityComparer<T2>? cmpB = null) : this(cmpA, cmpB)
    {
        foreach (var (a, b) in items)
        {
            _forward[a] = b;
            _reverse[b] = a;
        }
    }
    public T1 this[T2 v] => _reverse[v];
    public T2 this[T1 v] => _forward[v];
    public (T1 a, T2 b) Insert(T1 a, T2 b)
    {
        _forward[a] = b;
        _reverse[b] = a;
        return (a, b);
    }
    public T1 GetOrAdd(T2 val, Func<T1> f) => _reverse.GetOrAdd(val, f);
    public T2 GetOrAdd(T1 val, Func<T2> f) => _forward.GetOrAdd(val, f);
    public bool TryGetValue(T1 val, out T2 ret ) => _forward.TryGetValue(val, out ret);
    public bool TryGetValue(T2 val, out T1 ret ) => _reverse.TryGetValue(val, out ret);
    
}

public static class DoubleDictExtensions
{
    public static T1 GetOrAdd<T1, T2>(this IDoubleDict<T1, T2> dd, T2 val, T1 newValue) where T1:IComparable
        => dd.GetOrAdd(val, ()=>newValue);
    public static T2 GetOrAdd<T1, T2>(this IDoubleDict<T1, T2> dd, T1 val, T2 newValue) where T1:IComparable
        => dd.GetOrAdd(val, ()=>newValue);
    public static  T1 Get<T1, T2>(this IDoubleDict<T1, T2> dd, T2 val) where T1:IComparable => dd[val];
    public static  T2 Get<T1, T2>(this IDoubleDict<T1, T2> dd, T1 val) where T1:IComparable => dd[val];
}

public static class DoubleDict
{
    
    public static DoubleDict<string, T2> Oic<T2>() => new(StringComparer.OrdinalIgnoreCase);
    public static DoubleDict<string, string> OicOic() => new(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
}