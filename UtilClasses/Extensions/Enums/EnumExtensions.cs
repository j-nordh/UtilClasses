using System.Linq;
using UtilClasses.Extensions.Reflections;
using UtilClasses.Extensions.Strings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UtilClasses.Extensions.Integers;

namespace UtilClasses.Extensions.Enums
{
    public static class EnumExtensions
    {
        public static bool IsSuccess(this System.Net.HttpStatusCode code) => ((int) code) >= 200 && ((int) code) <= 299;

        public static T ParseAs<T>(this string s, bool ignoreCase=true)
        {
            if(!typeof(T).IsEnum) throw new Exception($"The supplied type {typeof(T).Name} is not an Enum.");
            return (T) Enum.Parse(typeof(T), s, ignoreCase);
        }

        public static List<T> Values<T>()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("This method can only be used with an Enum generic type parameter");
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        public static T CastTo<T>(this int i) where T:IEquatable<int>
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("This method can only be used with an Enum generic type parameter");

            if (!Enum.IsDefined(typeof(T), i))
                throw new ArgumentException($"The supplied integer {i} is not a valid value of {typeof(T).Name}");
            return Enum.GetValues(typeof(T)).Cast<T>().First(v => v.Equals(i));
        }

        public static T AsEnum<T>(this int i, T defaultValue) where T:struct 
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("This method can only be used with an Enum generic type parameter");

            if (!Enum.IsDefined(typeof(T), i))
                return defaultValue;
            return (T)(object)i;
        }
        public static T? AsEnum<T>(this int i, T? defaultValue) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("This method can only be used with an Enum generic type parameter");

            if (!Enum.IsDefined(typeof(T), i))
                return defaultValue;
            return (T)(object)i;
        }

        public static T? AsEnum<T>(this object o, T? defaultValue) where T : struct, Enum, IConvertible, IComparable => Enum<T>.TryParse(o) ?? defaultValue;

        public static T AsEnum<T>(this object o) where T : struct, Enum, IConvertible, IComparable
        {
            var ret = Enum<T>.TryParse(o);
            if(null == ret) throw new Exception($"Could not parse {o} into a {typeof(T).Name}");
            return ret.Value;
        }

        public static IEnumerable<T> WhereEnum<T>(this IEnumerable<T> hay, params T[] needles)
            where T : struct, IComparable => hay.WhereEnum(x => x, needles);

        public static IEnumerable<T2> WhereEnum<T, T2>(this IEnumerable<T2> hay, Func<T2, T> f, params T[] needles)
            where T : struct, IComparable
        {
            var set = new HashSet<T>(needles);
            return hay.Where(h=>set.Contains(f(h)));
        }

        public static TA? GetEnumAttribute<TA>(this object e) where TA:class=>
            e.GetType().GetMember(e.ToString())
                .FirstOrDefault()?
                .GetCustomAttributes(false)
                .OfType<TA>()
                .FirstOrDefault();

        public static string? GetKey<T>(this T e) where T:Enum => e.GetEnumAttribute<KeyAttribute>()?.Key;
        public static string GetKeyOrName<T>(this T e) where T : Enum => e.GetKey() ?? e.ToString();
        public static bool In<T>(this T needle, params T[] es) where T : Enum => es.Any(e=>e.Equals(needle));

    }

    public class Enum<T> where T : struct, Enum, IConvertible, IComparable
    {
        public static List<T> Values
        {
            get
            {
                RequireEnum();
                return Enum.GetValues(typeof(T)).Cast<T>().ToList();
            }
            
        }
        private static void RequireEnum()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("This method can only be used with an Enum generic type parameter");
        }
        public static List<string> Names
        {
            get
            {
                RequireEnum();
                return Enum.GetValues(typeof(T)).Cast<T>().Select(e => e.ToString()).ToList();
            }
        }
        public static int Count
        {
            get
            {
                RequireEnum();
                return Enum.GetValues(typeof(T)).Length;
            }
        }

        public static bool Is(T e, params T[] needles) => needles.Any(n=>e.CompareTo(n)==0);

        public static IEnumerable<string> DescOrNames => Values.Select(DescriptionOrName);

        public static string DescriptionOrName(T v)
        {
            RequireEnum();
            var memInfo = typeof(T).GetMember(v.ToString());
            var a = memInfo[0].GetFirstCustomAttribute<DescriptionAttribute>();
            return null ==a? v.ToString(): a.Description;
        }
        public static T Cast(int i, T defaultValue) 
        {
            RequireEnum();

            if (!Enum.IsDefined(typeof(T), i))
                return defaultValue;
            return (T)(object)i;
        }
        public static T Cast(int i)
        {
            RequireEnum();

            if (!Enum.IsDefined(typeof(T), i))
                throw new ArgumentOutOfRangeException($"{i} cannot be converted to a {typeof(T).Name}");
            return (T)(object)i;
        }
        public static IEnumerable<T> WithAttribute<TA>(Func<TA?, bool> pred) where TA : Attribute => Values.Where(e => pred(e.GetEnumAttribute<TA>()));
        public static IEnumerable<T> WithAttribute<TA>() where TA : Attribute => Values.Where(e => e.GetEnumAttribute<TA>()!=null);
        public static T FirstOrDefault<TA>(Func<TA?, bool> pred) where TA : Attribute => WithAttribute(pred).FirstOrDefault();

        public static T Match<TA>(string pred) where TA : Attribute, IStringMatchable => FirstOrDefault<TA>(sm => sm?.Matches(pred)??false);
        
        public static T Parse(object o)
        {
            var s = o.ToString().Trim();
            if (s.IsInt()) return (T)(object)s.AsInt();
            return s.ParseAs<T>();
        }
        public static T? TryParse(object? o)
        {
            if (null == o) return null;
            var s = o as string ?? o.ToString();
            if (s.IsInt()) return s.AsInt().AsEnum<T>(null);
            if (Enum.TryParse<T>(s, out var res)) return res;


            foreach (var v in Values)
            {
                var key = typeof(T).GetMember(v.ToString()).FirstOrDefault()?.CustomAttributes.FirstOrDefaultConstructorArg<string>("Key");
                if (null == key) continue;
                if (key.EqualsOic(s))
                    return v;
            }
            return null;
        }

        public static bool Valid(object o) => TryParse(o) != null;

        public static byte AsByte(T v) => Convert.ToByte(v);
    }
}
