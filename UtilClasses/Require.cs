using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Enumerables;

namespace UtilClasses
{
    public class Require
    {
        bool _res = true;
        public bool Valid => _res;

        public Require NotNullOrEmpty(params string[] ss) => Apply(ss, s => s.IsNotNullOrEmpty());
        public Require Not<T>(T forbidden, params T[] vals) where T : IEquatable<T> => Apply(vals, v => !v.Equals(forbidden));
        public Require Not(bool val) => Apply(new[] { () => !val });
        public Require EqualsOic(string required, params object[] os) => Apply(os, o=>o.ToString().EqualsOic(required));
        public Require True(Func<bool> f) => Apply(new[] { f });
        private Require Apply<T>(IEnumerable<T> os, Func<T, bool> f) => Apply(os.Select<T,Func<bool>>(o=>()=>f(o)));
        private Require Apply(IEnumerable<Func<bool>> fs)
        {
            try
            {
                _res = _res && fs.All(f => f() == true);
            }
            catch
            {
                _res = false;
            }
            return this;
        }
        
    }
}
