using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Extensions.Types
{
    public static class TypeExtensions
    {
        public static bool IsNumericType(this System.Type T)
        {
            var nt = Nullable.GetUnderlyingType(T);
            if (nt != null)
                T = nt;
            switch (System.Type.GetTypeCode(T))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static string SaneName(this Type t)
        {

            if (t.Name.StartsWithOic("Task"))
                return t.GenericTypeArguments.Any() ? SaneName(t.GenericTypeArguments.First()) : "void";
            return !t.IsGenericType
                ? t.Name
                : $"{t.Name.Substring(0, t.Name.Length - 2)}<{SaneName(t.GetGenericArguments().First())}>";
        }

        public static Type AsNullable(this Type t, bool nullable = true)
        {
            if (t.IsNullable())
                return nullable ? t : Nullable.GetUnderlyingType(t);

            if (t.IsClass) return t;
            if (t.IsValueType && nullable) return typeof(Nullable<>).MakeGenericType(t);
            return t;
        }

        public static bool IsNullable(this System.Type T)
        {
            return Nullable.GetUnderlyingType(T) != null;
        }

        public class PropFieldInformation
        {
            public PropFieldInformation(string name, System.Type valueType)
            {
                Name = name;
                ValueType = valueType;
            }

            public PropFieldInformation(PropertyInfo pi) : this(pi.Name, pi.PropertyType)
            {
            }

            public PropFieldInformation(FieldInfo fi) : this(fi.Name, fi.FieldType) { }

            public string Name { get; }
            public System.Type ValueType { get; }
        }

        public static PropFieldInformation GetPropFieldInfo(this System.Type t, string name)
        {
            var prop = t.GetProperties().FirstOrDefault(p => p.Name.EqualsIc2(name));
            if (null != prop) return new PropFieldInformation(prop.Name, prop.PropertyType);
            var field = t.GetFields().FirstOrDefault(f => f.Name.EqualsIc2(name));
            return field == null ? null : new PropFieldInformation(field.Name, field.FieldType);
        }

        public static IEnumerable<PropFieldInformation> GetPublicPropFieldInfo(this System.Type t) => t
            .GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => new PropFieldInformation(p))
            .Concat(t.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => new PropFieldInformation(f)));

        public static bool Is<T>(this System.Type t) => t == typeof(T);
        public static bool CanBe<T>(this Type t) => typeof(T).IsAssignableFrom(t);
        public static bool CanBe(this Type t, Type other) => other.IsAssignableFrom(t);

        public static IEnumerable<Type> Implementing(this IEnumerable<Type> ts, string ifaceName) => ts.Where(t => t.GetInterfaces().Any(i => i.Name.SubstringBefore("`").EqualsOic(ifaceName)));
        public static IEnumerable<Type> Implementing<T>(this IEnumerable<Type> ts) => ts.Where(t => t.GetInterfaces().Any(i => i.Name.SubstringBefore("`").EqualsOic(typeof(T).Name)));

        public static Func<TIn, TOut> GenerateConstructor<TIn, TOut>(this Type ot)
        {
            var arg = Expression.Parameter(typeof(TIn), "p1");
            var ctor = ot.GetConstructor(new[] { typeof(TIn) });
            if (null == ctor) return null;
            var exp = Expression.New(ctor, arg);
            LambdaExpression lambda = Expression.Lambda<Func<TIn, TOut>>(exp, arg);
            return (Func<TIn, TOut>)lambda.Compile();
        }
        public static Func<T1, T2, TOut> GenerateConstructor<T1, T2, TOut>(this Type ot)
        {
            var arg1 = Expression.Parameter(typeof(T1), "p1");
            var arg2 = Expression.Parameter(typeof(T2), "p2");
            var ctor = ot.GetConstructor(new[] { typeof(T1), typeof(T2) });
            if (null == ctor) return null;
            var exp = Expression.New(ctor, arg1, arg2);
            LambdaExpression lambda = Expression.Lambda<Func<T1, T2, TOut>>(exp, arg1, arg2);
            return (Func<T1, T2, TOut>)lambda.Compile();
        }
        public static Func<T1, T2, T3, TOut> GenerateConstructor<T1, T2, T3, TOut>(this Type ot)
        {
            var arg1 = Expression.Parameter(typeof(T1), "p1");
            var arg2 = Expression.Parameter(typeof(T2), "p2");
            var arg3 = Expression.Parameter(typeof(T3), "p3");
            var ctor = ot.GetConstructor(new[] { typeof(T1), typeof(T2), typeof(T3) });
            if (null == ctor) return null;
            var exp = Expression.New(ctor, arg1, arg2, arg3);
            LambdaExpression lambda = Expression.Lambda<Func<T1, T2, T3, TOut>>(exp, arg1, arg2, arg3);
            return (Func<T1, T2, T3, TOut>)lambda.Compile();
        }
        public static Func<T1, T2, T3, T4, TOut> GenerateConstructor<T1, T2, T3, T4, TOut>(this Type ot)
        {
            var arg1 = Expression.Parameter(typeof(T1), "p1");
            var arg2 = Expression.Parameter(typeof(T2), "p2");
            var arg3 = Expression.Parameter(typeof(T3), "p3");
            var arg4 = Expression.Parameter(typeof(T4), "p4");
            var ctor = ot.GetConstructor(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
            if (null == ctor) return null;
            var exp = Expression.New(ctor, arg1, arg2, arg3, arg4);
            LambdaExpression lambda = Expression.Lambda<Func<T1, T2, T3, T4, TOut>>(exp, arg1, arg2, arg3, arg4);
            return (Func<T1, T2, T3, T4, TOut>)lambda.Compile();
        }
        public static Func<TOut> GenerateConstructor<TOut>(this Type ot)
        {
            var ctor = ot.GetConstructor(new Type[] { });
            if (null == ctor) return null;
            var exp = Expression.New(ctor);
            LambdaExpression lambda = Expression.Lambda<Func<TOut>>(exp);
            return (Func<TOut>)lambda.Compile();
        }


        public static List<Accessor<TObj, T>>? GetPropAccessors<TObj, T>(this TObj obj, Func<PropertyInfo, bool>? predicate = null)
        {
            predicate ??= x => true;
            if (null == obj)
                return null;
            return obj.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(T))
                .Where(predicate)
                .Select(Accessor.FromPropInfo<TObj, T>)
                .ToList();
        }

        public static List<Accessor<TObj, bool>> GetFlagAccessors<TObj>(this TObj obj, Func<PropertyInfo, bool>? predicate = null) => obj.GetPropAccessors<TObj, bool>(predicate);


        public static IEnumerable<Type> RequireInterface<TInterface>(this IEnumerable<Type> types) => types.Where(t =>
            t.GetInterfaces().Any(i => i.FullName?.Equals(typeof(TInterface).FullName) ?? false));

        public static IEnumerable<Type> RequireNotAbstract(this IEnumerable<Type> types) =>
            types.Where(t => !t.IsAbstract);

        public static IEnumerable<Type> RequireNew(this IEnumerable<Type> types) =>
            types.Where(t => t.GetConstructors().Any(c => c.GetParameters().Length == 0));

        public static IEnumerable<Type> RequireConstructor(this IEnumerable<Type> types, params Type[] parameters) => types.Where(t => t.HasConstructor(parameters));

        public static IEnumerable<Type> RequireConstructor<T1>(this IEnumerable<Type> types) => types.RequireConstructor(typeof(T1));
        public static IEnumerable<Type> RequireConstructor<T1, T2>(this IEnumerable<Type> types) => types.RequireConstructor(typeof(T1), typeof(T2));

        public static IEnumerable<T> Activate<T>(this IEnumerable<Type> types) =>
            types
                .Select(Activator.CreateInstance)
                .Cast<T>();

        public static Type FirstOrDefault_MatchName(this IEnumerable<Type> types, Type template) =>
            types.FirstOrDefault(t => t.FullName?.Equals(template.FullName) ?? false);

        public static bool HasConstructor<T>(this Type type) => type.HasConstructor(typeof(T));
        public static bool HasConstructor<T, T1>(this Type type) => type.HasConstructor(typeof(T), typeof(T1));
        public static bool HasConstructor<T, T1, T2>(this Type type) => type.HasConstructor(typeof(T), typeof(T1), typeof(T2));
        public static bool HasConstructor<T, T1, T2, T3>(this Type type) => type.HasConstructor(typeof(T), typeof(T1), typeof(T2), typeof(T3));
        public static bool HasConstructor<T, T1, T2, T3, T4>(this Type type) => type.HasConstructor(typeof(T), typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public static bool HasConstructor(this Type type, params Type[] ps)
        {
            foreach (var c in type.GetConstructors())
            {
                var cps = c.GetParameters();
                if (cps.Length != ps.Length) continue;
                bool ok = true;
                for (int i = 0; i < cps.Length; i++)
                {
                    if (cps[i].ParameterType == ps[i]) continue;
                    ok = false;
                    break;
                }
                if (!ok) continue;
                return true;
            }
            return false;
        }

        public static IEnumerable<PropertyInfo> WhereNullable(this IEnumerable<PropertyInfo> pis) => pis.Where(pi => pi.PropertyType.IsNullable());
        public static IEnumerable<Accessor<T, object>> GetAccessors<T>(this IEnumerable<PropertyInfo> pis) => pis.Select(x => Accessor.FromPropInfo<T, object>(x));
        private static readonly IDictionary<string, string?> _typeMapping = new DictionaryOic<string?>()
        {
            {"String","string"},
            {"Int","int"},
            {"Int32","int"},
            {"Int64","long"},
            {"Void",null},
            {"Single", "float" },
            {"Boolean", "bool" }
        };
        public static string? MapTypeName(this string typeName) =>
            null == typeName
                ? null
                : _typeMapping.Maybe(typeName, typeName);

        public static string MapTypeName(this Type t) => MapTypeName(t?.Name);
    }
}
