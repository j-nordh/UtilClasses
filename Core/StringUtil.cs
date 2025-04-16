using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core;

public static class StringUtil
{
    public static string? Combine(char separator, bool trim, params string[]? parts)
    {
        if (parts == null) return null;
        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            if (part.IsNullOrEmpty()) continue;
            sb.Append(trim ? part.Trim(separator) : part);
            sb.Append(separator);
        }

        return sb.ToString();
    }

    public static string? Combine(char separator, params string[] parts) => Combine(separator, true, parts);
    private static List<(Func<string, bool> Predicate, Func<string, string> Transform)> _singleTransforms;

    static StringUtil()
    {
        _singleTransforms = new List<(Func<string, bool>, Func<string, string>)>();
        EndsWith("ies", s => Trim(3)(s) + "y");
        EndsWith("status", 0);
        EndsWith("ues", 1);
        EndsWith("ses", 2);
        EndsWith("xes", 2);
        EndsWith("ess", 0);
        EndsWith("s", (1));
    }

    private static void EndsWith(string ending, Func<string, string> f)
    {
        _singleTransforms.Add((s => s.EndsWithIc2(ending), f));
    }

    private static void EndsWith(string ending, int i) => EndsWith(ending, Trim(i));

    private static Func<string, string> Trim(int i) => s => s.Substring(0, s.Length - i);

    public static string ToSingle(string plural)
    {
        if (plural.StartsWith("tbl"))
            plural = plural.SubstringAfter("tbl");
        foreach (var t in _singleTransforms.Where(t => t.Predicate(plural)))
        {
            return t.Transform(plural);
        }

        return plural;
    }

    public static string FixPluralization(string plural)
    {
        if (plural.StartsWith("tbl"))
            plural = plural.SubstringAfter("tbl");
        if (plural.EndsWithAnyOic("ays", "oys")) return plural;
        if (plural.EndsWithOic("ys"))
            plural = plural.Substring(0, plural.Length - 2) + "ies";
        if (plural.EndsWithIc2("sss"))
            plural = plural.Substring(0, plural.Length - 1);
        if (plural.EndsWithOic("ss"))
            plural += "es";
        return plural;
    }

    public static (string, string) FixPluralization(string a, string b) =>
        (FixPluralization(a), FixPluralization(b));

    public static (string, string, string) FixPluralization(string a, string b, string c) =>
        (FixPluralization(a), FixPluralization(b), FixPluralization(c));

    public static string TrimInterfaceIndicator(string s)
    {
        if (s.Length < 2) return s;
        var tmp = s.Substring(0, 2);
        if (tmp[0] == 'I' && char.IsUpper(tmp[1]))
            return s.Substring(1);
        return s;
    }

    public static string PadCenter(string a, string b, int totalLength)
    {
        var spaces = new string(' ', totalLength - a.Length - b.Length);
        return $"{a}{spaces}{b}";
    }

    public static string SnakeToCamel(string s)
    {
        if (s.Any(char.IsUpper))
            return s;
        s = s.ToLower();
        var b = new StringBuilder();
        if (s.StartsWith("_"))
            s = s.Substring(1);
        b.Append(char.ToUpper(s.First()));
        bool nextUpper = false;
        foreach (var c in s.Skip(1))
        {
            if (c == '_')
            {
                nextUpper = true;
                continue;
            }

            b.Append(nextUpper ? char.ToUpper(c) : c);
            nextUpper = false;
        }

        return b.ToString();
    }
}