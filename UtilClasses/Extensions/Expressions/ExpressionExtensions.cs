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

        public static Action<TEntity, TProperty> CreateSetter<TEntity, TProperty>(this
            Expression<Func<TEntity, TProperty>> property)
        {
            PropertyInfo propertyInfo = GetProperty(property);

            ParameterExpression instance = Expression.Parameter(typeof(TEntity), "instance");
            ParameterExpression parameter = Expression.Parameter(typeof(TProperty), "param");

            var setMethod = propertyInfo.GetSetMethod();
            var body = Expression.Call(instance, setMethod, parameter);
            var parameters = new ParameterExpression[] { instance, parameter };

            return Expression.Lambda<Action<TEntity, TProperty>>(body, parameters).Compile();
        }

        public static PropertyInfo GetProperty<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            var member = GetMemberExpression(expression).Member;
            var property = member as PropertyInfo;
            if (property == null)
            {
                throw new InvalidOperationException(string.Format("Member with Name '{0}' is not a property.",
                    member.Name));
            }

            return property;
        }

        private static MemberExpression GetMemberExpression<TEntity, TProperty>(this
            Expression<Func<TEntity, TProperty>> expression)
        {
            MemberExpression memberExpression = null;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression.Body;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException("Not a member access", "expression");
            }

            return memberExpression;
        }

        public static string GetName<T, TVal>(this Expression<Func<T, TVal>> m) =>
            m.Body.MemberName() ?? m.Name ?? "Unknown";

        public static Func<TEntity, TProperty> CreateGetter<TEntity, TProperty>(
            this Expression<Func<TEntity, TProperty>> property)
        {
            PropertyInfo propertyInfo = GetProperty(property);

            ParameterExpression instance = Expression.Parameter(typeof(TEntity), "instance");

            var body = Expression.Call(instance, propertyInfo.GetGetMethod());
            var parameters = new ParameterExpression[] { instance };

            return Expression.Lambda<Func<TEntity, TProperty>>(body, parameters).Compile();
        }

        public static MemberInfo GetMember(this Expression e)
        {
            if (e is not MemberExpression me)
            {
                //try interpreting it as a lambda expression
                var lambda = e as LambdaExpression;
                if (lambda == null)
                    throw new ArgumentNullException(nameof(e), @"That's not even a lambda expression!");
                //try casting it directly as a MemberExpression
                me = lambda.Body as MemberExpression;

                //special case we _can_ support but probably don't use
                if (lambda.Body.NodeType == ExpressionType.Convert)
                    me = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }

            if (me == null) throw new ArgumentException("Expressions must be on the form ()=>object.Property");
            return me.Member;
        }
    }

    public static class Express
    {
        public static DictionaryBuilder<TKey, TVal> Dict<TKey, TVal>(IEqualityComparer<TKey>? comp = null) => new(comp);

        public static DictionaryBuilder<string, TVal> StringDict<TVal>(StringComparer? comp = null) =>
            Dict<string, TVal>(comp ?? StringComparer.OrdinalIgnoreCase);


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