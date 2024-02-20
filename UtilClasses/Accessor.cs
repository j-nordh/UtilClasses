using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UtilClasses.Extensions.Expressions;

namespace UtilClasses
{
    public static class Accessor
    {
        public static Accessor<T, TObj> FromPropInfo<T, TObj>(PropertyInfo pi)
            => new Accessor<T, TObj> { Get = GetGetter<T, TObj>(pi.Name), Set = GetSetter<T, TObj>(pi.Name) };

        public static Accessor<TObj, T> FromPropertyName<TObj, T>(string name)
            => new Accessor<TObj, T> { Get = GetGetter<TObj, T>(name), Set = GetSetter<TObj, T>(name) };

        public static Accessor<TObj, T> FromExpression<TObj, T>(Expression<Func<TObj, T>> e) => FromPropertyName<TObj, T>(e.GetMember().Name);

        private static Action<TObj, T> GetSetter<TObj, T>(string propName)
        {
            var exp1 = Expression.Parameter(typeof(TObj));
            var exp2 = Expression.Parameter(typeof(T), propName);
            MemberExpression prop = Expression.Property(exp1, propName);
            return Expression.Lambda<Action<TObj, T>>
            (
                Expression.Assign(prop, exp2), exp1, exp2
            ).Compile();
        }
        private static Func<TObj, T> GetGetter<TObj, T>(string propName)
        {
            var paramExpression = Expression.Parameter(typeof(TObj), "value");
            var propertyGetterExpression = Expression.Property(paramExpression, propName);
            return Expression.Lambda<Func<TObj, T>>(propertyGetterExpression, paramExpression).Compile();
        }
    }

    public static class AccessorExtensions
    {
        public static IEnumerable<object?> GetValues<T>(this IEnumerable<Accessor<T, object>> accessors, T obj) => accessors.Select(a => a.Get?.Invoke(obj));
    }

    public class Accessor<TObj, T>
    {
        public Func<TObj, T>? Get { get; set; }
        public Action<TObj, T>? Set { get; set; }
    }
}
