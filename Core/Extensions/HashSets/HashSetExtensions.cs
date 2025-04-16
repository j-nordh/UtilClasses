using System;
using System.Collections.Generic;

namespace UtilClasses.Core.Extensions.HashSets;

public static class HashSetExtensions
{
    public static bool ContainsAll<T>(this HashSet<T> set, params T[] vals)
    {
        foreach (var v in vals)
        {
            if (!set.Contains(v)) return false;
        }
        return true;
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items, IEqualityComparer<T> comp) => new(items, comp);
        
    public static HashSet<string> ToHashSetOic(this IEnumerable<string> items) => new(items, StringComparer.OrdinalIgnoreCase);
    public static HashSet<string> ToHashSet(this IEnumerable<string> items) => new(items);
    //public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items) => new(items);
    public static HashSet<T> Clone<T>(this HashSet<T> set) => set == null ? null : new HashSet<T>(set, set.Comparer);
}