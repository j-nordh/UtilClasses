using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using UtilClasses.Extensions.Decimals;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Doubles;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Expressions;
using UtilClasses.Extensions.Integers;
using UtilClasses.Extensions.Lists;
using UtilClasses.Extensions.Objects;
using UtilClasses.Files;
using static UtilClasses.Extensions.Strings.Chunk;

namespace UtilClasses.Extensions.Strings
{
    /// <summary>
    /// Extensions for <see cref="System.String"/>
    /// </summary>
    public static class StringExtensions
    {
        static Dictionary<Type, (Func<string, bool> Check, Func<string, object> Conv)> _typeConverters =
            new Dictionary<Type, (Func<string, bool>, Func<string, object>)>();

        static StringExtensions()
        {
            SetConv(IsInt, IntegerExtensions.AsInt);
            SetConv(DoubleExtensions.IsDouble, DoubleExtensions.AsDouble);
            SetConv(DecimalExtensions.IsDecimal, DecimalExtensions.AsDecimal);
            SetConv(IntegerExtensions.IsLong, IntegerExtensions.AsLong);
            SetConv(_ => true, AsBoolean);
            SetConv(IsDateTime, AsDateTime);
            SetConv(IsDayOfWeek, AsDayOfWeek);
            SetConv(DoubleExtensions.IsFloat, DoubleExtensions.AsFloat);
            SetConv(IsGuid, AsGuid);
            SetConv(_ => true, s => s);
        }

        static void SetConv<T>(Func<string, bool> check, Func<string, T> conv)
        {
            _typeConverters[typeof(T)] = (check, s => conv(s));
        }

        /// <summary>
        /// A nicer way of calling <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
        [ContractAnnotation("value:null => true")]
        public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
        
        /// <summary>
        /// A nicer way of calling the inverse of <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is not null or an empty string (""); otherwise, false.</returns>
        [ContractAnnotation("value:null=>false")]
        public static bool IsNotNullOrEmpty(this string? value) => !value.IsNullOrEmpty();

        public static bool IsNotNullOrWhitespace(this string? s) => !s.IsNullOrWhitespace();

        public static bool IsEmpty(this string value)
        {
            return value.Equals(string.Empty);
        }

        public static bool IsNullOrWhitespace(this string? s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static string CrLf(this string s) => s.RemoveAll("\r").Replace("\n", "\r\n");
        public static string Lf(this string s) => s.RemoveAll("\r");

        /// <summary>
        /// A nicer way of calling <see cref="System.String.Format(string, object[])"/>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return System.String.Format(format, args);
        }

        /// <summary>
        /// Allows for using strings in null coalescing operations
        /// </summary>
        /// <param name="value">The string value to check</param>
        /// <returns>Null if <paramref name="value"/> is empty or the original value of <paramref name="value"/>.</returns>
        public static string NullIfEmpty(this string value)
        {
            if (value == System.String.Empty)
                return null;

            return value;
        }

        /// <summary>
        /// Separates a PascalCase string
        /// </summary>
        /// <example>
        /// "ThisIsPascalCase".SeparatePascalCase(); // returns "This Is Pascal Case"
        /// </example>
        /// <param name="value">The value to split</param>
        /// <returns>The original string separated on each uppercase character.</returns>
        public static string SeparatePascalCase(this string value)
        {
            Ensure.Argument.NotNullOrEmpty(value, "value");
            return Regex.Replace(value, "([A-Z])", " $1").Trim();
        }


        /// <summary>
        /// Returns a string array containing the trimmed substrings in this <paramref name="value"/>
        /// that are delimited by the provided <paramref name="separators"/>.
        /// </summary>
        public static IEnumerable<string> SplitAndTrim(this string value, params char[] separators)
        {
            Ensure.Argument.NotNull(value, "source");
            return value.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
        }

        public static IEnumerable<string> SplitAndTrim(this string value, string separator)
        {
            Ensure.Argument.NotNull(value, "source");
            return value.Trim().Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())
                .Where(s => s.IsNotNullOrEmpty());
        }

        /// <summary>
        /// Checks if the <paramref name="source"/> contains the <paramref name="input"/> based on the provided <paramref name="comparison"/> rules.
        /// </summary>
        public static bool Contains(this string? source, string? input, StringComparison comparison)
        {
            if (source == null) return false;
            if (input == null) return false;
            return source.IndexOf(input, comparison) >= 0;
        }

        /// <summary>
        /// Limits the length of the <paramref name="source"/> to the specified <paramref name="maxLength"/>.
        /// </summary>
        public static string Limit(this string source, int maxLength, string? suffix = null)
        {
            if (suffix.IsNotNullOrEmpty())
            {
                maxLength = maxLength - suffix?.Length ?? 0;
            }

            if (source.Length <= maxLength) return source;

            return string.Concat(source.Substring(0, maxLength).Trim(), suffix ?? string.Empty);
        }

        public static bool EndsWithIc2(this string s, string s2)
        {
            return s?.EndsWith(s2, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }

        public static bool EndsWithOic(this string s, string end) =>
            s.EndsWith(end, StringComparison.OrdinalIgnoreCase);

        public static bool EndsWithIc2(this string s, IEnumerable<string> endings) => endings.Any(s.EndsWithIc2);

        public static bool EndsWithIc2(this string s, params string[] endings) => endings.Any(s.EndsWithIc2);

        public static bool StartsWithAnyOic(this string s, IEnumerable<string> needles) =>
            s.StartsWithAnyOic(needles.ToArray());

        public static bool StartsWithAnyOic(this string s, params string[] needles) =>
            needles.Any(n => s.StartsWith(n, StringComparison.OrdinalIgnoreCase));

        public static bool StartsWithOic(this string? s, string? needle) => null == s
            ? null == needle
            : needle != null && s.StartsWith(needle, StringComparison.OrdinalIgnoreCase);

        public static bool SwitchOnStartOic(this string s, params (string needle, Action<string> a)[] paths) =>
            s.SwitchOn(StartsWithOic, false, paths);

        public static bool SwitchOnStartOic(this string s, bool matchMultiple,
            params (string needle, Action<string> a)[] paths)
            => s.SwitchOn(StartsWithOic, matchMultiple, paths);


        public static bool SwitchOn(this string s, Func<string, string, bool> matcher, bool matchMultiple,
            params (string needle, Action<string> a)[] paths)
        {
            var ret = false;
            foreach (var p in paths)
            {
                if (!matcher(s, p.needle))
                    continue;
                p.a(s);
                ret = true;
                if (!matchMultiple) break;
            }

            return ret;
        }

        public static string StripJsonTag(this string s)
        {
            if (!s.StartsWithOic("json<")) return s;
            if (!s.EndsWith(">")) return s;
            return s.Substring(5, s.Length - 6);
        }

        public static string StripAllGenerics(this string s)
        {
            if (null == s) return null;
            var start = s.LastIndexOf('<');
            if (start == -1) return s;
            var end = s.IndexOf('>', start);
            if (end == -1) return s;
            return s.Substring(start + 1, end - start - 1);
        }

        public static bool ContainsOic(this string? s, string s2)
        {
            return s?.Contains(s2, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public static int CompareToIc2(this string value1, string value2) =>
            string.Compare(value1, value2, StringComparison.InvariantCultureIgnoreCase);

        public static int CompareToCc(this string s1, string s2)
            => string.Compare(s1, s2, StringComparison.CurrentCulture);

        public static string ReplaceOic(this string s, string oldValue, string newValue)
        {
            return ReplaceOic(s, oldValue, newValue, out _);
        }

        public static string ReplaceOic(this string s, string oldValue, string newValue, out int count) =>
            s.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase, out count);

        public static string ReplaceO(this string s, string oldValue, string newValue)
        {
            return ReplaceO(s, oldValue, newValue, out _);
        }

        public static string ReplaceO(this string s, string oldValue, string newValue, out int count) =>
            s.Replace(oldValue, newValue, StringComparison.Ordinal, out count);

        private static string Replace(this string s, string oldValue, string newValue, StringComparison sc,
            out int count)
        {
            count = 0;
            newValue ??= "";
            if (s.IsNullOrEmpty()) return s;
            if (oldValue.IsNullOrEmpty()) return s;
            if (oldValue.EqualsIc2(newValue)) return s;
            if (oldValue.Length > s.Length) return s;
            int foundAt = 0;
            while ((foundAt = s.IndexOf(oldValue, foundAt > 0 ? foundAt + newValue.Length : 0, sc)) != -1)
            {
                s = s.Remove(foundAt, oldValue.Length).Insert(foundAt, newValue);
                count += 1;
            }

            return s;
        }

        public static string ReplaceBetweenOic(this string s, string startTag, string endTag,
            Func<string, string> replacer)
        {
            if (s.IsNullOrEmpty()) return s;
            var start = s.IndexOfOic(startTag);
            if (start < 0) throw new ArgumentException("Could not find start tag.");
            start += startTag.Length;
            var end = s.IndexOfOic(endTag, start);
            if (end < 0) throw new ArgumentException("Could not find end tag after start tag.");
            return s.Substring(0, start) + replacer(s.Substring(start, end - start)) + s.Substring(end);
        }

        public static bool NotIn(this string needle, params string[] hay) => !hay.Any(h => h.EqualsOic(needle));

        public static string InsertAfter(this string s, string marker, string val)
        {
            var i = s.IndexOf(marker) + marker.Length;
            return s.Insert(i, val);
        }

        public static string EnsureCase(this string s, string correct)
        {
            var tmp = Guid.NewGuid().ToString();
            return s.ReplaceOic(correct, tmp).Replace(tmp, correct);
        }

        public static string Clean(this string s, IEnumerable<string> forbidden, string replacement)
        {
            var sb = new StringBuilder(s);
            foreach (var f in forbidden)
            {
                sb.Replace(f, replacement);
            }

            return sb.ToString();
        }

        public static string RemoveAll(this string s, params string[] needles) =>
            needles.Aggregate(s, (current, n) => current.Replace(n, ""));

        public static string RemoveAllOic(this string s, params string[] needles) =>
            needles.Aggregate(s, (current, n) => current.ReplaceOic(n, ""));

        public static string RemoveAllWhitespace(this string s)
            => s.RemoveAll(" ", "\t", "\r", "\n", Convert.ToChar(160).AsString());

        public static string RemoveEndOic(this string s, string end) =>
            s.EndsWithOic(end) ? s.Substring(0, s.Length - end.Length) : s;

        public static bool EqualsIc2(this string? s, string? value)
        {
            if (null == s) return value == null;
            return s.Equals(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EqualsOic(this string? s, string? value)
        {
            if (null == s) return value == null;
            return s.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsAnyOic(this string? s, IEnumerable<string> vals) =>
            vals?.Any(s.EqualsOic) ?? false;


        public static bool EqualsAnyOic(this string? s, params string[] vals) => s.EqualsAnyOic(vals.AsEnumerable());

        [Obsolete("Use EqualsAnyOic instead")]
        public static bool InOic(this string s, IEnumerable<string> vals) =>
            vals?.Any(s.EqualsOic) ?? false;


        [Obsolete("Use EqualsAnyOic instead")]
        public static bool InOic(this string? s, params string[] vals) => s.EqualsAnyOic(vals.AsEnumerable());

        public static bool AsBoolean(this string? s)
        {
            if (s.IsNullOrEmpty()) return false;
            if (bool.TryParse(s, out bool ret)) return ret;
            if (int.TryParse(s, out int i)) return i != 0;
            if (s.EqualsOic("true")) return true;
            if (s.EqualsOic("yes")) return true;
            if (s.EqualsOic("yea")) return true;
            if (s.EqualsOic("ja")) return true;
            if (s.EqualsOic("japp")) return true;
            if (s.EqualsOic("visst")) return true;
            if (s.EqualsOic("kanske")) return false;
            if (s.ContainsOic("huvudvärk")) return false;
            return false;
        }

        public static DateTime AsDateTime(this string s)
        {
            if (DateTime.TryParse(s, out var ret)) return ret;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out ret))
                return ret;
            throw new ArgumentException($"Could not parse \"{s}\" as a DateTime");
        }

        public static bool IsDateTime(this string s) => DateTime.TryParse(s, out var _);

        public static DateTime? MaybeAsDateTime(this string s) =>
            DateTime.TryParse(s, out var ret) ? (DateTime?)ret : null;

        public static DateTime? MaybeAsDateTime(this string s, string format) =>
            DateTime.TryParseExact(s, format, CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeLocal,
                out var ret)
                ? (DateTime?)ret
                : null;

        public static Guid AsGuid(this string s) => Guid.Parse(s);
        public static bool IsGuid(this string s) => Guid.TryParse(s, out var _);


        public static T As<T>(this string s)
        {
            var conv = _typeConverters.GetOrThrow(typeof(T),
                () => throw new NotImplementedException($"No converter specified for type {typeof(T).Name}"));
            return (T)conv.Conv(s);
        }

        public static T? MaybeAs<T>(this string s) where T : struct
        {
            if (s.IsNullOrWhitespace()) return null;
            var (Check, Conv) = _typeConverters.GetOrThrow(typeof(T),
                () => throw new NotImplementedException($"No converter specified for type {typeof(T).Name}"));
            if (!Check(s)) return null;
            return (T)Conv(s);
        }

        public static byte? MaybeAsByte(this string s) => byte.TryParse(s, out var ret) ? (byte?)ret : null;
        public static byte? MaybeAsByte(this string s, bool predicate) => predicate ? s.MaybeAsByte() : null;

        public static T AsEnum<T>(this string s, bool ignoreCase = true)
        {
            if (!typeof(T).IsEnum) throw new System.Exception($"The supplied type {typeof(T).Name} is not an Enum.");
            return (T)Enum.Parse(typeof(T), s, ignoreCase);
        }

        private static readonly Dictionary<string, DayOfWeek> _dayDict =
            new Dictionary<string, DayOfWeek>(StringComparer.OrdinalIgnoreCase)
            {
                ["måndag"] = DayOfWeek.Monday,
                ["tisdag"] = DayOfWeek.Tuesday,
                ["onsdag"] = DayOfWeek.Wednesday,
                ["torsdag"] = DayOfWeek.Thursday,
                ["fredag"] = DayOfWeek.Friday,
                ["lördag"] = DayOfWeek.Saturday,
                ["söndag"] = DayOfWeek.Sunday,
                ["monday"] = DayOfWeek.Monday,
                ["tuesday"] = DayOfWeek.Tuesday,
                ["wednesday"] = DayOfWeek.Wednesday,
                ["thursday"] = DayOfWeek.Thursday,
                ["friday"] = DayOfWeek.Friday,
                ["saturday"] = DayOfWeek.Saturday,
                ["sunday"] = DayOfWeek.Sunday
            };

        public static DayOfWeek AsDayOfWeek(this string s) => _dayDict[s];
        public static bool IsDayOfWeek(this string s) => _dayDict.ContainsKey(s);

        public static int IndexOfIc2(this string str, string needle)
        {
            if (null == str || null == needle) return -1;
            return str.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase);
        }

        public static int IndexOfIc2(this string str, string needle, int start)
        {
            if (null == str || null == needle) return -1;
            return str.IndexOf(needle, start, StringComparison.InvariantCultureIgnoreCase);
        }

        public static int IndexOfOic(this string str, string needle)
        {
            if (null == str || null == needle) return -1;
            return str.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        }

        public static int IndexOfOic(this string str, string needle, int start)
        {
            if (null == str || null == needle) return -1;
            return str.IndexOf(needle, start, StringComparison.OrdinalIgnoreCase);
        }

        public static string[] SplitREE(this string str, params char[] cs)
        {
            return str.Split(cs, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitREE(this string str, params string[] ss)
        {
            return str.Split(ss, StringSplitOptions.RemoveEmptyEntries);
        }


        public static string[] SplitLines(this string str, bool removeEmptyEntries = false)
        {
            if (null == str)
                return new string[] { };
            return str.Replace("\r", "").Split(new[] { "\n" },
                removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        }

        public static IEnumerable<string> Trim(this IEnumerable<string> items) => items.Select(s => s.Trim());

        public static string TrimLines(this string str, string lineBreak = "\r\n") =>
            str.SplitLines().Trim().Join(lineBreak);

        public static int LineCount(this string str)
        {
            return str.SplitLines().Length;
        }

        public static string SubstringAfter(this string str, char needle, int length = -1) =>
            str.SubstringAfter($"{needle}", length);

        public static string SubstringAfter(this string str, string needle, int length = -1)
        {
            if (null == str) return null;
            int index = str.IndexOfIc2(needle);

            if (index < 0) index = 0;
            else index += needle.Length;
            return length > 0 ? str.Substring(index, length) : str.Substring(index);
        }

        public static string SubstringStartingWith(this string str, string needle,
            StringComparison sc = StringComparison.OrdinalIgnoreCase)
        {
            int i = str.IndexOf(needle, sc);
            return i < 0 ? null : str.Substring(i);
        }

        public static string SubstringAfterLast(this string str, string needle)
        {
            int index = str.LastIndexOf(needle, StringComparison.OrdinalIgnoreCase);
            index = index < 0 ? 0 : index + needle.Length;
            return str.Substring(index);
        }


        public static string SubstringBefore(this string str, char needle) =>
            SubstringBefore(str, new[] { $"{needle}" }, out _);

        public static string SubstringBefore(this string str, string needle) =>
            SubstringBefore(str, new[] { needle }, out _);

        public static string SubstringBeforeLast(this string str, string needle,
            StringComparison sc = StringComparison.OrdinalIgnoreCase)
        {
            var index = str.LastIndexOf(needle, sc);
            return index < 0 ? str : str.Substring(0, index);
        }

        public static string SubstringBetweenQuotes(this string str,
            StringComparison sc = StringComparison.OrdinalIgnoreCase) => str.SubstringBetween("\"", "\"", sc);
        public static string SubstringBetween(this string str, string before, string after,
            StringComparison sc = StringComparison.OrdinalIgnoreCase)
        {
            var start = str.IndexOf(before, sc);
            var end = str.IndexOf(after, start+1, sc);
            if (start == -1) throw new ArgumentException("Could not find \"before\" marker");
            if (end == -1) throw new ArgumentException("Could not find \"after\" marker");

            start += before.Length;
            return str.Substring(start, end - start);
        }

        public static string SubstringBefore(this string str, char needle, out string rest, bool keepMarker = true) =>
            str.SubstringBefore($"{needle}", out rest, keepMarker);

        public static string SubstringBefore(this string str, string needle, out string rest, bool keepMarker = true)
            => str.SubstringBefore(new[] { needle }, out rest, keepMarker);

        public static string SubstringBefore(this string str, IEnumerable<string> needles, bool keepMarker = true)
        {
            return SubstringBefore(str, needles, out _, keepMarker);
        }

        public static int IndexOfAny(this string str, IEnumerable<string> needles, out string? matchingNeedle)
        {
            int ret = int.MaxValue;
            matchingNeedle = null;
            foreach (var needle in needles)
            {
                var index = str.IndexOfIc2(needle);
                if (index < 0) continue;
                if (index < ret)
                {
                    ret = index;
                    matchingNeedle = needle;
                }
            }

            return int.MaxValue == ret ? -1 : ret;
        }

        public static string SubstringBefore(this string str, IEnumerable<string> needles, out string rest,
            bool keepMarker = true)
        {
            var index = str.IndexOfAny(needles, out string? match);
            if (index < 0 || null == match)
            {
                rest = "";
                return str;
            }

            rest = keepMarker ? str.Substring(index) : str.Substring(index + match.Length);
            return str.Substring(0, index);
        }

        public static string TrimBefore(this string str, string needle)
        {
            int index = str.IndexOfIc2(needle);
            if (index < 0) return "";
            return index == 0 ? str : str.Substring(index);
        }

        public static string TrimAll(this string str, params char[] needles) => str.TrimAll(true, needles);

        public static string TrimAll(this string str, bool whitespace, params char[] needles)
        {
            int len;
            do
            {
                len = str.Length;
                if (whitespace)
                    str = str.Trim();
                foreach (var n in needles)
                {
                    str = str.Trim(n);
                }
            } while (len > str.Length);

            return str;
        }

        public static string TrimEnd(this string str, string end,
            StringComparison sc = StringComparison.OrdinalIgnoreCase)
            => str.EndsWith(end, sc) ? str.Substring(0, str.Length - end.Length) : str;

        public static string TrimStart(this string str, string start,
            StringComparison sc = StringComparison.OrdinalIgnoreCase)
            => str.StartsWith(start, sc) ? str.Substring(start.Length) : str;

        public static int NumberAfter(this string str, string needle)
        {
            return int.Parse(
                new string(
                    str.SubstringAfter(needle)
                        .Trim()
                        .TakeWhile(char.IsNumber).ToArray()));
        }

        public static string Shuffle(this string str)
        {
            var chars = str.ToCharArray().ToList();
            chars.Shuffle();
            return new string(chars.ToArray());
        }

        public static IEnumerable<string> Permutations(this string str)
        {
            if (str.Length == 1) yield return str;
            else
            {
                for (int i = 0; i < str.Length; i++)
                {
                    var rest = str.Pop(i, out char c);
                    foreach (var p in rest.Permutations())
                    {
                        yield return c + p;
                    }
                }
            }
        }

        public static string Pop(this string str, int index, out char c)
        {
            if (index < 0 || index > str.Length) throw new IndexOutOfRangeException();
            c = str[index];
            return str.Remove(index, 1);
        }

        public static bool Has(this string str, string letters, bool ignoreCase = true)
        {
            int start = 0;
            if (ignoreCase)
            {
                str = str.ToLower();
                letters = letters.ToLower();
            }

            foreach (var letter in letters)
            {
                var index = str.IndexOf(letter, start);
                if (index == -1) return false;
                start = index + 1;
            }

            return true;
        }

        public static void WriteLines(this IEnumerable<string> strings, TextWriter? wr = null)
        {
            wr ??= Console.Out;
            foreach (var s in strings)
            {
                wr.WriteLine(s);
            }
        }

        public static void AddFormatted(this List<string> list, string format, params object[] ps)
        {
            list.Add(string.Format(format, ps));
        }

        public static bool ContainsAny(this string value, IEnumerable<string> needles)
        {
            return needles.Any(n => value.ToLower().Contains(n.ToLower()));
        }

        public static bool ContainsAny(this string value, params string[] needles) =>
            value.ContainsAny(needles.AsEnumerable());

        public static bool ContainsANumber(this string value) =>
            value.ContainsAny(Enumerable.Range(0, 10).Select(i => i.ToString()));

        public static bool ContainsAll(this string value, params string[] needles) =>
            value.ContainsAll(needles.AsEnumerable());

        public static bool ContainsAll(this string value, IEnumerable<string> needles)
        {
            return needles.All(n => value.ToLower().Contains(n.ToLower()));
        }

        public static bool ContainsLineParts(this string val, char sep, params string[] parts) =>
            ContainsLineParts(val, sep, true, StringComparison.OrdinalIgnoreCase, parts);

        public static bool ContainsLineParts(this string val, char sep, bool trim, StringComparison sc, string[] parts)
        {
            foreach (var line in val.SplitLines())
            {
                var ps = line.Split(sep);
                if (ps.Length != parts.Length)
                    continue;
                for (var i = 0; i < ps.Length; i++)
                {
                    var a = trim ? ps[i].Trim() : ps[i];
                    var b = trim ? parts[i].Trim() : parts[i];
                    if (a.Equals(b, sc))
                        return true;
                }
            }

            return false;
        }


        public static string Join(this IEnumerable<string> parts, string separator) =>
            string.Join(separator, parts.ToArray());

        public static string Join(this IEnumerable<string> parts, Func<string, string> sep)
        {
            var lst = parts.ToList();
            var sb = new StringBuilder();
            for (int i = 0; i < lst.Count; i++)
            {
                sb.Append(lst[i]);
                if (i < lst.Count - 1)
                    sb.Append(sep(lst[i]));
            }

            return sb.ToString();
        }

        public static string JoinAndIndent(this IEnumerable<string> parts, string indent) =>
            indent + string.Join("\r\n" + indent, parts.ToArray());

        public static string JoinAndIndent(this IEnumerable<string> parts, string separator, string indent) =>
            indent + string.Join(separator + "\r\n" + indent, parts.ToArray());

        public static Makers MakeIt(this string str) => new Makers(str);

        public class Makers
        {
            private readonly string _str;

            public Makers(string str)
            {
                _str = str;
            }

            public string EndWith(char c) => _str.EndsWith("" + c) ? _str : _str + c;
            public string StartWithLowerCase => $"{char.ToLower(_str.First())}{_str.Substring(1)}";
            public string StartWithUpperCase => $"{char.ToUpper(_str.First())}{_str.Substring(1)}";

            public string CamelCase()
            {
                var str = PascalCase();
                return $"{char.ToLower(str[0])}{str.Substring(1)}";
            }

            public string PascalCase()
            {
                if (_str.IsNullOrEmpty()) return _str;

                var splitPoints = new[] { ' ', '_' };
                var count = _str.Count(splitPoints.Contains);
                if (count > 0)
                    return _str
                        .SplitREE(splitPoints)
                        .Where(s => s.Length > 0)
                        .Select(s => char.ToUpper(s[0]) + s.Substring(1).ToLower())
                        .Join("");

                var upperCount = _str.Count(char.IsUpper);
                var lowerCount = _str.Count(char.IsLower);

                if (upperCount == 0 || lowerCount == 0) return char.ToUpper(_str[0]) + _str.Substring(1).ToLower();
                return
                    char.ToUpper(_str[0]) +
                    _str.Substring(
                        1); //It's probably in PascalCase already, let's just ensure the first letter is upper case...
            }
        }

        public static string CapitalizeFirst(this string s)
        {
            if (s.IsNullOrEmpty()) return s;
            if (s.Length == 1) return s.ToUpper();
            return s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower();
        }

        public static IEqualityComparer<string> ToEqualityComparer(this StringComparison c) => ToStringComparer(c);

        public static StringComparer ToStringComparer(this StringComparison c)
        {
            switch (c)
            {
                case StringComparison.CurrentCulture: return StringComparer.CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase: return StringComparer.CurrentCultureIgnoreCase;
                case StringComparison.InvariantCulture: return StringComparer.InvariantCulture;
                case StringComparison.InvariantCultureIgnoreCase: return StringComparer.InvariantCultureIgnoreCase;
                case StringComparison.Ordinal: return StringComparer.Ordinal;
                case StringComparison.OrdinalIgnoreCase: return StringComparer.OrdinalIgnoreCase;
                default:
                    throw new ArgumentOutOfRangeException(nameof(c), c, null);
            }
        }

        public static T WhenTrimmed<T>(this string s, Func<string, T> f) => f(s.Trim());
        public static bool IsJson(this string s) => s.HasMarkers(('[', ']'), ('{', '}'));
        public static bool IsJsonArray(this string s) => s.HasMarkers('[', ']');
        public static bool IsJsonObject(this string str) => str.HasMarkers('{', '}');
        public static bool HasMarkers(this string str, char start, char end) => str.HasMarkers((start, end));

        public static bool HasMarkers(this string str, params (char start, char end)[] markers) =>
            str.IsNotNullOrEmpty()
            && str.WhenTrimmed(s => markers.Any(m => s.First() == m.start && s.Last() == m.end));

        public static bool IsXml(this string s) => !s.IsNullOrEmpty() && s[0] == '<' && s.Last() == '>';

        public static bool IsInt(this string s) => s.All(c => !char.IsLetter(c)) && int.TryParse(s, out int i);


        public static bool IsEnum<T>(this string s) where T : struct
        {
            return Enum.TryParse(s, true, out T v);
        }

        public static bool IsUri(this string s, UriKind kind = UriKind.Absolute) => s.AsUri(kind) != null;

        public static Uri AsUri(this string s, UriKind kind = UriKind.Absolute, params string[] schemes)
        {
            if (s.IsNullOrWhitespace()) return null;
            if (!Uri.TryCreate(s, kind, out Uri ret)) return null;
            return schemes.Any() ? (schemes.Any(scheme => ret.Scheme.Equals(scheme)) ? ret : null) : ret;
        }

        public static string WhenNullOrEmpty(this string str, string alt) => str.IsNullOrEmpty() ? alt : str;

        public static string WhenNullOrEmpty(this string str, Func<string> onTrue, Func<string> onFalse) =>
            str.When(str.IsNullOrEmpty(), onTrue, onFalse);

        public static string WhenNullOrEmpty(this string str, string onTrue, string onFalse) =>
            str.When(str.IsNullOrEmpty(), onTrue, onFalse);

        public static string AppendNotNull<T>(this string s, T? o, Func<T, string> f, Func<string>? nullValue = null)
            where T : class
        {
            if (null == o)
                return nullValue == null ? s : s + nullValue();
            return nullValue + f(o);
        }

        public static string Append(this string s, string o, Func<string, bool> predicate, Func<string, string> f,
            string falseValue = "")
        {
            if (predicate(o)) return s + f(o);
            return s + falseValue;
        }

        public static string AppendNNW(this string s, string o, Func<string, string> f, string falseValue = "") =>
            Append(s, o, m => !m.IsNullOrWhitespace(), f, falseValue);

        public static string AppendIfNotNullOrEmpty(this string s, string o, Func<string, string> f,
            string? nullValue = null)
        {
            if (o.IsNullOrEmpty())
                return nullValue == null ? s : s + nullValue;
            return s + f(o);
        }

        public static byte[] HexToByteArray(this string s)
        {
            var dict = new Dictionary<char, byte>()
            {
                { '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 }, { '4', 4 }, { '5', 5 },
                { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 }, { 'A', 10 }, { 'B', 11 },
                { 'C', 12 }, { 'D', 13 }, { 'E', 14 }, { 'F', 15 }, { 'a', 10 }, { 'b', 11 },
                { 'c', 12 }, { 'd', 13 }, { 'e', 14 }, { 'f', 15 }
            };

            foreach (var c in s.Where(c => !dict.ContainsKey(c)).Distinct().ToList())
            {
                var parts = s.Split(c);
                if (parts.Any(p => p.Length > 2))
                {
                    s = s.Replace(c.ToString(), "");
                    continue;
                }

                s = parts.Select(p => p.Length == 2 ? p : "0" + p).Join("");
            }

            int len = s.Length / 2;
            var ret = new byte[len];
            bool isFirst = true;
            byte b = 0;
            int i = 0;
            foreach (var c in s)
            {
                if (isFirst)
                {
                    b = (byte)(dict[c] << 4);
                    isFirst = false;
                }
                else
                {
                    b |= dict[c];
                    ret[i] = b;
                    i += 1;
                    isFirst = true;
                }
            }

            //for (int i = 0; i < len; i++)
            //{
            //    ret[i] = (byte)(dict[s[i << 1]] << 4 + dict[s[i << 1 + 1]]);
            //}
            return ret;
        }

        public static bool EndsWithAnyOic(this string s, IEnumerable<string> items) => items.Any(s.EndsWithOic);
        public static bool EndsWithAnyOic(this string s, params string[] items) => items.Any(s.EndsWithOic);

        public static byte[] ToUtf8(this string s) => Encoding.UTF8.GetBytes(s);

        public static string ToBase64(this string s) => s.ToBase64(Encoding.UTF8);
        public static string ToBase64(this string s, Encoding enc) => Convert.ToBase64String(enc.GetBytes(s));
        public static string FromBase64(this string s) => s.FromBase64(Encoding.UTF8);
        public static string FromBase64(this string s, Encoding enc) => enc.GetString(Convert.FromBase64String(s));

        public static IEnumerable<Chunk> GetChunks(this string s, char startDelimiter, char stopDelimiter,
            bool onlyTopChunks)
            => Chunk.FromString(s, startDelimiter, stopDelimiter, onlyTopChunks);

        public static ChunkGetter GetChunks(this string s) => new Chunk(new StringBuilder(s)).Get;

        public static int CountOic(this string hay, string needle)
        {
            int count = 0;
            int start = 0;
            while (true)
            {
                var current = hay.IndexOfOic(needle, start);
                if (current == -1)
                    return count;
                count += 1;
                start = current + needle.Length;
            }
        }

        private static List<(string str, string xml)> _xmlChars = new()
        {
            ("\"", "&quot;"),
            ("'", "&apos;"),
            ("<", "&lt;"),
            (">", "&gt;"),
            ("&", "&amp;")
        };

        private static List<(string str, string control)> _controlChars = new()
        {
            ("\\r", "\r"),
            ("\\n", "\n"),
            ("\\t", "\t"),
        };

        public static string EscapeForXml(this string input) =>
            _xmlChars.Aggregate(input, (s, r) => s.ReplaceOic(r.str, r.xml));

        public static string RestoreFromXml(this string input) =>
            _xmlChars.Aggregate(input, (s, r) => s.ReplaceOic(r.xml, r.str));

        public static string EscapeForSql(this string input) =>
            input.ReplaceOic("''", "'").ReplaceOic("'", "''");

        public static string EscapeControlChars(this string input) =>
            _controlChars.Aggregate(input, (s, c) => s.ReplaceOic(c.control, c.str));

        public static string RestoreControlChars(this string input) =>
            _controlChars.Aggregate(input, (s, c) => s.ReplaceOic(c.str, c.control));

        public static bool IsNullOrEquals(this string pattern, string candidate,
            StringComparison sc = StringComparison.Ordinal) =>
            pattern == null && candidate == null ||
            pattern != null && candidate != null && candidate.Equals(pattern, sc);

        public static bool IsNullOrEqualsOic(this string pattern, string candidate) =>
            pattern.IsNullOrEquals(candidate, StringComparison.OrdinalIgnoreCase);

        public static bool IsNullOrIn(this string pattern, string candidate) =>
            pattern == null || candidate.ContainsOic(pattern);

        public static bool IsNullOrAny(this string pattern, params string[] candidates) =>
            pattern.IsNullOrAny(StringComparison.OrdinalIgnoreCase, candidates);

        public static bool IsNullOrAny(this string pattern, StringComparison sc, params string[] candidates) =>
            pattern == null || candidates.Any(c => pattern.Equals(c, sc));

        //public static bool SaveIfChanged(this string content, string path, bool force = false, bool removeWhitespace = true, bool ignoreCase = true, Encoding encoding = null)
        //{
        //    if (null == encoding) encoding = Encoding.UTF8;
        //    if (!IsChanged(content, path, encoding, force, removeWhitespace, ignoreCase)) return false;
        //    File.WriteAllText(path, content, encoding);
        //    return true;
        //}

        //private static bool IsChanged(string content, string path, Encoding encoding, bool force, bool removeWhitespace, bool ignoreCase)
        //{
        //    if (force) return true;
        //    if (!File.Exists(path)) return true;
        //    var org = File.ReadAllText(path, encoding);
        //    if (removeWhitespace)
        //    {
        //        content = content.RemoveAllWhitespace();
        //        org = org.RemoveAllWhitespace();
        //    }
        //    return !content.Equals(org, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        //}

        public static FileSaver GetSaver(this string content, string path) => new FileSaver(path).WithContent(content);

        public static void SplitAssign(this string str, string delimiter, out string first, out string second,
            bool trim = true)
        {
            var parts = str.SplitREE(delimiter);
            first = parts[0];
            second = parts[1];
            if (!trim) return;
            first = first.Trim();
            second = second.Trim();
        }

        public static void SplitAssign(this string str, string delimiter, Action<string> first, Action<string> second,
            bool trim = true)
        {
            str.SplitAssign(delimiter, out var a, out var b, trim);
            first(a);
            second(b);
        }

        public static void SplitAssign(this string str, string delimiter, object owner, Expression<Func<string>> first,
            Expression<Func<string>> second, bool trim = true)
        {
            str.SplitAssign(delimiter, out var a, out var b, trim);
            first.Set(owner, a);
            second.Set(owner, b);
        }

        public static (int start, int length) NextGroup(this string str, int startIndex = 0)
        {
            char startChar = ' ';
            char endChar = ' ';
            var charDict = new Dictionary<char, char>()
            {
                ['('] = ')',
                ['['] = ']',
                ['{'] = '}',
                ['<'] = '>'
            };
            for (int i = startIndex; i < str.Length; i++)
            {
                if (charDict.TryGetValue(str[i], out endChar))
                {
                    startChar = str[i];
                    break;
                }
            }

            if (startChar == ' ')
            {
                return (-1, 0);
            }

            int count = 0;
            int groupStart = 0;
            for (int i = startIndex; i < str.Length; i++)
            {
                if (str[i] == startChar)
                {
                    if (count == 0)
                        groupStart = i;
                    count++;
                }

                if (str[i] == endChar)
                {
                    count--;
                    if (count == 0)
                        return (groupStart, i - groupStart + 1);
                }
            }

            return (-1, 0);
        }

        private static readonly List<(char, char)> _matchingPairs = new List<(char, char)>
            { ('{', '}'), ('[', ']'), ('(', ')') };

        private static char GetMatchingChar(char c)
        {
            foreach (var (a, b) in _matchingPairs)
            {
                if (a == c) return b;
                if (b == c) return a;
            }

            throw new ArgumentException($"Could not find matching character for: {c}");
        }

        public static int FindClosing(this string s, char c, int start = 0, int initialCount = 1)
        {
            char inc = GetMatchingChar(c);
            int count = initialCount;
            for (int i = start; i < s.Length; i++)
            {
                var x = s[i];
                if (x == c) count -= 1;
                if (x == inc) count += 1;
                if (count == 0) return i;
            }

            return -1;
        }
    }
}