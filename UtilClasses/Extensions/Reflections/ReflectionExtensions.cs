using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Decimals;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Exceptions;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Extensions.Reflections
{
    public static class ReflectionExtensions
    {
        public static bool HasAttribute(this MethodInfo mi, string attrName)
        {
            if (!attrName.EndsWithIc2("Attribute")) attrName += "Attribute";
            return mi.CustomAttributes.Any(a => a.AttributeType.Name.EqualsIc2(attrName));
        }
        public static bool HasAttribute(this ParameterInfo pi, string attrName)
        {
            if (!attrName.EndsWithIc2("Attribute")) attrName += "Attribute";
            return pi.CustomAttributes.Any(a => a.AttributeType.Name.EqualsIc2(attrName));
        }

        public static IEnumerable<CustomAttributeData> WhereTypeName(this IEnumerable<CustomAttributeData> items,
            Func<string, bool> predicate) => items.Where(i => predicate(i.AttributeType.Name));

        public static IEnumerable<CustomAttributeData> WhereTypeNameContains(
            this IEnumerable<CustomAttributeData> items, string name,
            StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
            => items.WhereTypeName(n => n.Contains(name, sc));

        public static CustomAttributeData FirstOrDefault(this IEnumerable<CustomAttributeData> items, string name,
            StringComparison sc = StringComparison.OrdinalIgnoreCase)
            => items.WhereTypeNameContains(name, sc).FirstOrDefault();

        public static T FirstOrDefaultConstructorArg<T>(this IEnumerable<CustomAttributeData> items, string name,
            StringComparison sc = StringComparison.OrdinalIgnoreCase) where T : class =>
            items.FirstOrDefault(name, sc)?.ConstructorArguments.GetFirst<T>();

        public static string GetAssemblyVersion(this Type t) => Assembly.GetAssembly(t).GetName().Version.ToString();

        public static T GetValue<T>(this IList<CustomAttributeNamedArgument> args, string name,
            StringComparison sc = StringComparison.InvariantCultureIgnoreCase) where T : class
        {
            return args.FirstOrDefault(a => a.MemberName.Equals(name, sc)).TypedValue.Value as T;
        }

        public static int? GetIntValue(this IList<CustomAttributeNamedArgument> args, string name,
            StringComparison sc = StringComparison.InvariantCultureIgnoreCase) => args.GetValue<object>(name, sc) as int?;

        public static T GetFirst<T>(this IList<CustomAttributeTypedArgument> args) where T : class => args.First().Value as T;
        public static T GetFirstOrDefault<T>(this IList<CustomAttributeTypedArgument> args) where T : class => args.FirstOrDefault().Value as T;

        public static T? FirstOrDefault<T>(this IList<CustomAttributeTypedArgument> args) where T : class =>
            args.IsNullOrEmpty() ? null : args.First().Value as T;
        public static string FirstConstructorArg(this CustomAttributeData c) =>
            c.ConstructorArguments.GetFirst<string>();

        public static T? GetFirstCustomAttribute<T>(this MemberInfo mi, bool inherit = false) =>
            (T?)mi.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        public static T? GetFirstCustomAttribute<T>(this Type t, string memberName, bool inherit = false) =>
            (T?)t.GetMember(memberName).FirstOrDefault()?.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        public static bool HasCustomAttribute<T>(this MemberInfo mi, bool inherit = false) =>
            mi.GetCustomAttributes(typeof(T), inherit).Any();
        public static bool HasCustomAttribute<T>(this MethodInfo mi, bool inherit = false) =>
            mi.GetCustomAttributes(typeof(T), inherit).Any();
        public static bool HasCustomAttribute(this MethodInfo mi, Type t, bool inherit = false) =>
            mi.GetCustomAttributes(t, inherit).Any();

        private static Dictionary<Type, Func<object, string>> _stringFuncs;

        static ReflectionExtensions()
        {
            _stringFuncs = new Dictionary<Type, Func<object, string>>()
                .Add<int>(i => i.ToString())
                .Add<string>(s => s)
                .Add<decimal>(d => d.ToSaneString())
                .Add<double>(d => d.ToString())
                .Add<IPAddress>(ip => ip.ToString());

        }

        private static Dictionary<Type, Func<object, string>> Add<T>(this Dictionary<Type, Func<object, string>> d,
            Func<T, string> f)
        {
            d[typeof(T)] = o => f((T)o);
            return d;
        }

        private  static string InternalRenderer(string name, object? o, Func<string, object, string> fallback)
        {
            return o switch
            {
                null => "null",
                Guid g => g.ToString(),
                int i => i.ToString(),
                string s => s,
                ulong ul => ul.ToString(),
                bool b => b.ToString(),
                decimal d=>d.ToSaneString(3),
                _ => fallback(name, o)
            };
        }
        public static string ReflectionToString(this object o, Func<string, object, string> renderer, int indent = 0)
        {


            var sb = new IndentingStringBuilder("  ");
            try
            {
                var f = _stringFuncs.Maybe(o.GetType());
                if (null != f) return f(o);
                sb.AppendLines($"({o.GetType()})");
                var t = o.GetType();
                if (t.Name.StartsWithOic("List`"))
                {
                    foreach (var c in (IEnumerable)o)
                    {
                        sb.AppendLines(ReflectionToString(c, renderer, indent + 1));
                    }
                }
                var props = o.GetType().GetProperties();
                var fields = o.GetType().GetFields();



                sb.AppendLines(props.Select(pi => $"{pi.Name}:  {InternalRenderer(pi.Name, pi.GetValue(o), renderer)}"));
                sb.AppendLines(fields.Select(fi => $"{fi.Name}: {InternalRenderer(fi.Name, fi.GetValue(o), renderer)} "));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.DeepToString());
                throw;
            }

            return sb.ToString();
        }
    }
}
