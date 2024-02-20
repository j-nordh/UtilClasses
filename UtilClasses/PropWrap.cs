using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Dictionaries;

namespace UtilClasses
{
    public class PropWrap<TObj, TProp>
    {
        private readonly Func<TObj> _obj;
        private readonly PropertyInfo? _prop;
        public string Name { get; private set; }

        public PropWrap(Func<TObj> obj, Expression<Func<TObj, TProp>> e)
        {
            _obj = obj;
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
            _prop = typeof(TObj).GetProperties().Single(p => p.Name.Equals(me.Member.Name));
            Name = me.Member.Name;
        }

        public PropWrap(Func<TObj> obj, string name)
        {
            Name = name;
            _obj = obj;
            _prop = typeof(TObj).GetProperty(name);
        }

        public TProp? Get() => _obj() == null
            ? default
            : (TProp?)_prop?.GetValue(_obj());
        public void Set(TProp val)
        {
            var o = _obj();
            if (o == null) throw new NullReferenceException();
            _prop.SetValue(_obj(), val);
        }
    }
}
