using System.Collections.Generic;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Extensions.Hashing;

public static class HashingExtensions
{
    public static int HashContent<T>(this IEnumerable<T> items, int seed) where T:class
    {
        if (items.IsNullOrEmpty()) return seed;
        int hash = seed;
        unchecked
        {
            foreach (var a in items)
            {
                hash = hash.AddHash(a);
            }
        }
        return hash;
    }

    public static int AddHash<T>(this int hash, IEnumerable<T> items) where T : class
    {
        return items.HashContent(hash);
    }

    public static int AddHash(this int hash, string s)
    {
        if (s.IsNullOrEmpty()) return hash;
        return hash * 17 + s.GetHashCode();
    }

    public static int AddHash<T>(this int hash, T o) where T : class
    {
        if (null == o) return hash;
        unchecked
        {
            return hash *17 + o.GetHashCode();
        }
    }
}