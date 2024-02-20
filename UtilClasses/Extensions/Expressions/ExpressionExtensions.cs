using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UtilClasses.Extensions.Expressions
{
    public static class ExpressionExtensions
    {
        public static string MemberName(this Expression e) => e.GetMember().Name;

        public static void Set(this Expression e, object obj, object value)
        {
            var m = e.GetMember();
            if (m is PropertyInfo pi)
                pi.GetSetMethod(true).Invoke(obj, new[] { value });
            if (m is FieldInfo fi)
                fi.SetValue(obj, value);
        }

        public static T Get<T>(this Expression e, object obj)
        {
            switch (e.GetMember())
            {
                case PropertyInfo pi:
                    return (T)pi.GetGetMethod(true).Invoke(obj, new object[] { });
                case FieldInfo fi:
                    return (T)fi.GetValue(obj);
                default:
                    throw new Exception("Could not generate a suitable accessor");
            }
        }

        public static MemberInfo GetMember(this Expression e)
        {
            var lambda = e as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException(nameof(e), @"That's not even a lambda expression!");
            MemberExpression? me = null;

            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Convert:
                    me = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                    break;
                case ExpressionType.MemberAccess:
                    me = lambda.Body as MemberExpression;
                    break;
            }
            if (me == null) throw new ArgumentException("Expressions must be on the form ()=>object.Property");
            return me.Member;
        }
    }
    public static class Express
    {
        public static DictionaryBuilder<TKey, TVal> Dict<TKey, TVal>(IEqualityComparer<TKey>? comp = null) => new(comp);
        public static DictionaryBuilder<string, TVal> StringDict<TVal>(StringComparer? comp = null) => Dict<string, TVal>(comp ?? StringComparer.OrdinalIgnoreCase);
        

        public class DictionaryBuilder<TKey, TVal>
        {
            readonly Dictionary<TKey, Expression<Func<TVal>>> _ret;
            public DictionaryBuilder(IEqualityComparer<TKey>? comp)
            {
                _ret = comp == null 
                    ? new Dictionary<TKey, Expression<Func<TVal>>>() 
                    : new Dictionary<TKey, Expression<Func<TVal>>>(comp);
            }
            public Dictionary<TKey, Expression<Func<TVal>>> Done() => _ret;
            public DictionaryBuilder<TKey, TVal> On(TKey key, Expression<Func<TVal>> e)
            {

                _ret[key] = e;
                return this;
            }
        }
    }
}
