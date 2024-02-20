using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Types;

namespace UtilClasses
{
    class PropertyTransferer<T1,T2>
    {
        private List<TransferDefinition> _transfers = new List<TransferDefinition>();
        public PropertyTransferer<T1, T2> Set<T>(Expression<Func<T1, T>> e1, Expression<Func<T2, T>> e2)
        {
            var a = Accessor.FromExpression(e1);
            var b = Accessor.FromExpression(e2);
            _transfers.Add(new TransferDefinition<T>(a, b));
            return this;
        }

        public PropertyTransferer<T1, T2> UnsafeSet(Expression<Func<T1, object>> e1, Expression<Func<T2, object>> e2)
        {
            _transfers.Add(new TransferDefinition<object>(Accessor.FromExpression(e1), Accessor.FromExpression(e2)));
            return this;
        }

        public PropertyTransferer<T1, T2> SetByNameAndType()
        {
            var names = typeof(T1).GetProperties().Select(pi => pi.Name)
                .Intersect(typeof(T2).GetProperties().Select(pi => pi.Name));
            foreach (var name in names)
            {
                var t1 = typeof(T1).GetProperty(name)?.PropertyType;
                var t2 = typeof(T2).GetProperty(name)?.PropertyType;
                if(t1== null || t2 == null) continue;
                if(t1 != t2) continue;
                //sigh. Must cast down to object and back not to piss of type checker. Conditions above should suffice.
                var a = Accessor.FromPropertyName<T1, object>(name);
                var b = Accessor.FromPropertyName<T2, object>(name);
                _transfers.Add(new TransferDefinition<object>(a,b));
            }
            return this;
        }

        public T2 Transfer(T1 source, T2 target)
        {
            foreach (var t in _transfers)
            {
                t.Transfer(source, target);
            }
            return target;
        }
        public T1 Transfer(T2 source, T1 target)
        {
            foreach (var t in _transfers)
            {
                t.Transfer(source, target);
            }
            return target;
        }

        private abstract class TransferDefinition
        {
            public abstract void Transfer(T1 from, T2 to);
            public abstract void Transfer(T2 from, T1 to);
        }


        private class TransferDefinition<T>: TransferDefinition
        {
            private readonly Accessor<T1, T> _a;
            private readonly Accessor<T2, T> _b;

            public TransferDefinition(Accessor<T1, T> a, Accessor<T2, T> b)
            {
                _a = a;
                _b = b;
            }


            public override void Transfer(T1 from, T2 to) => _b.Set(to, _a.Get(@from));
            public override void Transfer(T2 from, T1 to) => _a.Set(to, _b.Get(@from));
        }
    }
}
