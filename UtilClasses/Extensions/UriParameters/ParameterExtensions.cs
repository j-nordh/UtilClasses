using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Extensions.UriParameters
{
    public static class ParameterExtensions
    {
        public static string RemoveParameter(this string s, string p)
        {
            return s.RemoveParameters(new[] { p });
        }

        public static string RemoveParameters(this string s, IEnumerable<string> removePs)
        {
            return ApplyFilter(s, p => RemoveParameterFilter(p, removePs));
        }

        public static string TranslateParameters(this string s, IDictionary<string, string> translatePs)
        {
            return s.ApplyFilter(p => TranslateParameterFilter(p, translatePs));
        }

        public static string RemoveAndTranslateParameters(this string s, IEnumerable<string> removePs, IDictionary<string, string> translatePs)
        {
            return ApplyFilters(s, new List<Func<string, string>>
            {
                p => RemoveParameterFilter(p, removePs), 
                p => TranslateParameterFilter(p, translatePs)
            });
        }

        [Pure]
        public static IEnumerable<string> GetParameterValues(this string str)
        {
            var kvs = str.SplitAndTrim('?', '&');
            return kvs.Select(s => s.Split('=')).Select(kv => kv.Count() == 2 ? kv[1] : null);
        }

        private static string ApplyFilter(this string s, Func<string, string> f)
        {
            return ApplyFilters(s, new[] { f }.ToList());
        }

        private static string ApplyFilters(this string s, List<Func<string, string>> fs)
        {
            var parts = s.Split('?', '&');
            var sb = new System.Text.StringBuilder();
            foreach (var part in parts)
            {
                if (part.IsNullOrWhitespace()) continue;
                string val = fs.Aggregate(part, (current, f) => f(current));
                if (null == val) continue;
                sb.Append(val);
                sb.Append('&');
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private static string RemoveParameterFilter(string p, IEnumerable<string> forbidden)
        {
            if (null == forbidden) return p;
            if (null == p) return null;
            return forbidden.Any(f => p.StartsWithOic(f + '=')) ? null : p;
        }

        private static string TranslateParameterFilter(string p, IDictionary<string, string> translations)
        {
            if (null == translations) return p;
            if (null == p) return null;
            foreach (var kv in translations.Where(kv => p.StartsWithOic(kv.Key + '=')))
            {
                return p.ReplaceOic(kv.Key + '=', kv.Value + '=');
            }
            return p;
        }
    }
}
