using System;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Extensions.Integers;

public static class IntegerExtensions
{
    public static void BetweenOrThrow<T>(this T i, object min, object max, string name) where T:IComparable
    {
        if (i.Between(min,max)) return;
        throw new ArgumentOutOfRangeException(name, $"Must be between{min} and {max}.");
    }
    public static bool Between<T>(this T i, object min, object max) where T : IComparable => i.CompareTo(min) > 0 && i.CompareTo(max) < 0;
    public static int AsInt(this string s) => int.Parse(s.SubstringBefore(".").SubstringBefore(",").ReplaceOic(" ","").Trim());
    public static ulong AsULong(this string s) => ulong.Parse(s.SubstringBefore(".").SubstringBefore(",").Trim());
    public static int AsInt(this object obj)
    {
        if (obj is int i)
            return i;
        if (obj is long l)
            return (int) l;
        return obj.ToString().AsInt();
    }

    public static int? MaybeAsInt(this object obj) => obj as int? ?? obj.ToString().MaybeAsInt();
    public static ulong? MaybeAsULong(this object obj) => obj as ulong? ?? obj.ToString().MaybeAsULong();
    public static ulong? MaybeAsULong(this string s) => ulong.TryParse(s, out var ret) ? ret : null;
    public static long? MaybeAsLong(this object obj) => obj as long? ?? obj.ToString().MaybeAsLong();
    public static int MaybeAsInt(this string s, Func<Exception> onError)
    {
        if (int.TryParse(s, out var ret)) return ret;
        throw onError();
    }
    public static ulong MaybeAsULong(this string s, Func<Exception> onError)
    {
        if (ulong.TryParse(s, out var ret)) return ret;
        throw onError();
    }
    public static int? MaybeAsInt(this string s) => s.IsNotNullOrEmpty() && int.TryParse(s, out var ret) ? ret : null;

    public static long AsLong(this string s) => long.Parse(s);
    public static bool IsLong(this string s) => long.TryParse(s, out var _);
    public static long? MaybeAsLong(this string s) => long.TryParse(s,out var ret) ? ret:null;
    public static long AsLong(this object obj) => obj as long?
                                                  ?? obj.MaybeAsLong()
                                                  ?? obj.ToString().AsLong();
    public static ulong AsULong(this object obj) =>
        obj switch
        {
            ulong ul => ul,
            int i => (ulong) Math.Abs(i),
            long l => (ulong) Math.Abs(l),
            _ => obj.ToString().AsULong()
        };
}