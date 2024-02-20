using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Strings;

namespace UtilClasses
{
    public static class StringUtil
    {
        public static string? Combine(char separator, bool trim, params string[]? parts)
        {
            if (parts == null) return null;
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                if(part.IsNullOrEmpty()) continue;
                sb.Append(trim ? part.Trim(separator) : part);
                sb.Append(separator);
            }
            return sb.ToString();
        }

        public static string? Combine(char separator, params string[] parts) => Combine(separator, true, parts);
        private static List<(Func<string, bool> Predicate, Func<string, string> Transform)> _singleTransforms;

        static StringUtil()
        {
            _singleTransforms= new List<(Func<string, bool>, Func<string, string>)>();
            EndsWith("ies",s=>Trim(3)(s)+"y");
            EndsWith("status", 0);
            EndsWith("ues", 1);
            EndsWith("ses", 2);
            EndsWith("xes", 2);
            EndsWith("s", Trim(1));
            
        }

        private static void EndsWith(string ending, Func<string, string> f)
        {
            _singleTransforms.Add((s=>s.EndsWithIc2(ending), f));
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
    }

    //public class StringSwitcher
    //{
    //    public Func<string, string, bool> DefaultMatcher { get; set; }
    //    public bool MatchMultiple { get; set; }
    //    public List<(string needle, Func<string, string, bool> matcher, Action<string> a)> Paths { get; set; }= new();
    //    public Func<string, string> PreProcess { get; set; } = s => s;
    //    public event Action<string, Exception> CaughtException;
    //    public StringSwitcher() { }
    //    public StringSwitcher(Func<string, Func<string, bool>> matcher)
    //    {
    //        DefaultMatcher = (a, b) => matcher(a)(b);
    //    }

    //    public bool Go(IEnumerable<string> ss) => ss.Aggregate(true, (res, s) => res &= Go(s));

    //    public bool Go(string s)
    //    {
    //        try
    //        {
    //            var ret = false;
    //            foreach (var p in Paths)
    //            {
    //                if (!p.matcher(s, p.needle))
    //                    continue;
    //                p.a(PreProcess(s));
    //                ret = true;
    //                if (!MatchMultiple) break;
    //            }
    //            return ret;
    //        }
    //        catch (Exception e)
    //        {
    //            if(null==CaughtException)
    //            {
    //                Console.WriteLine(e);
    //                throw;
    //            }
    //            CaughtException?.Invoke(s, e);
    //            return false;
    //        }
            
    //    }

    //    public StringSwitcher Add(string needle, Func<string, string, bool> matcher, Action<string> a)
    //    {
    //        Paths.Add((needle, matcher, a));
    //        return this;
    //    }

    //    public StringSwitcher Add(string needle, Action<string> a) => Add(needle, DefaultMatcher, a);
    //    public StringSwitcher Add(string needle, Action a) => Add(needle, DefaultMatcher, _=>a());
    //}

    //public class StringSwitcher<T>:StringSwitcher
    //{
    //    private readonly T _obj;
        

    //    public StringSwitcher(T obj)
    //    {
    //        _obj = obj;
    //    }
    //    public StringSwitcher(T obj, Func<string, Func<string, bool>> matcher) :base(matcher) 
    //    {
    //        _obj = obj;
    //    }
    //    public StringSwitcher<T> Add(string needle, Expression<Func<T, string>> e) => (StringSwitcher<T>)Add(needle, DefaultMatcher, x => Accessor.FromExpression(e).Set(_obj, x));
    //    public StringSwitcher<T> Add<T2>(string needle, Func<string, T2> conv, Expression<Func<T, T2>> e) => (StringSwitcher<T>)Add(needle, DefaultMatcher, x => Accessor.FromExpression(e).Set(_obj, conv(x)));
    //}
}
