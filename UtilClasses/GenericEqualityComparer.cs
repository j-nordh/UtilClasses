using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UtilClasses.Extensions.Objects;

namespace UtilClasses
{
    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly List<Func<T, object?>> _extractors;
        protected GenericEqualityComparer()
        {
            _extractors = new List<Func<T, object?>>();
        }

        protected T? _o;

        public static GenericEqualityComparer<T> FromProperties(bool onlyPublic = false)
        {
            var ret = new GenericEqualityComparer<T>();
            ret._extractors.AddRange(
                typeof(T).GetProperties()
                    .Where(p => (p.GetSetMethod()?.IsPublic ??false) && (p.GetGetMethod()?.IsPublic??false) || !onlyPublic)
                    .Select<PropertyInfo, Func<T, object>>(p => o => p.GetValue(o)));
            return ret;
        }

        public GenericEqualityComparer(params Expression<Func<T, object>>[] fs) : this()
        {
            Set(fs);
        }

        public GenericEqualityComparer<T> Set(params Expression<Func<object>>[] fs)
        {
            foreach (var f in fs)
            {
                _extractors.Add(GetExtractor(f));
            }
            return this;
        }
        public GenericEqualityComparer<T> Set(params Expression<Func<T, object>>[] fs)
        {
            foreach (var f in fs)
            {
                _extractors.Add(GetExtractor(f));
            }
            return this;
        }
        private static Func<T, object?> GetExtractor(Expression exp)
        {
            var lambda = exp as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("expression", @"That's not even a lambda expression!");

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
            switch (me.Member.MemberType)
            {
                case MemberTypes.Field:
                    return x => me.Member.As<FieldInfo>()?.GetValue(x);
                case MemberTypes.Property:
                    return x => me.Member.As<PropertyInfo>()?.GetValue(x);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public bool Equals(T x, T y)
        {
            if (x == null) return y == null;
            if (y == null) return false;
            return _extractors.All(ex =>
            {
                var xVal = ex(x);
                var yVal = ex(y);
                return null == xVal
                    ? null == yVal
                    : yVal != null && xVal.Equals(yVal);
            });
        }

        public int Compare([NotNull] T x, [NotNull] T y)
        {
            Ensure.Argument.NotNull(x, "Compare require that both arguments are not null");
            Ensure.Argument.NotNull(y, "Compare require that both arguments are not null");

            foreach (var ex in _extractors)
            {
                var xVal = ex(x);
                var yVal = ex(y);
                if (xVal == null || yVal == null)
                    throw new Exception("Value extraction failed");
                var res = ((IComparable)xVal).CompareTo(yVal);
                if (res != 0)
                    return res;
            }

            return 0;
        }

        public int GetHashCode(T obj)
        {
            if (null == obj) throw new ArgumentNullException();
            unchecked
            {
                return _extractors.Aggregate(0, (hash, extractor) => hash * 397 ^ extractor(obj)?.GetHashCode() ?? 0);
            }
        }
    }
}
