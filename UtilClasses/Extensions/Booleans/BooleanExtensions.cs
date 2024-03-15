using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilClasses.Extensions.Booleans
{
    public static class BooleanExtensions
    {
        public static bool IfTrue(this bool b, Action whenTrue, Action? whenFalse=null)
        {
            if (b) whenTrue();
            else whenFalse?.Invoke();
            return b;
        }
        public static bool IfTrue<T>(this bool b, T arg, Action<T> whenTrue, Action<T>? whenFalse = null)
        {
            if (b) whenTrue(arg);
            else whenFalse?.Invoke(arg);
            return b;
        }

        public static TOut? IfTrue<T, TOut>(this bool b, Func<T, TOut> whenTrue, T arg) where TOut : class =>
            b.IfTrue(whenTrue, null, arg);
        public static TOut? IfTrue<T, TOut>(this bool b, Func<T, TOut>? whenTrue, Func<T, TOut>? whenFalse, T arg) where TOut:class => 
            b ? whenTrue?.Invoke(arg) : whenFalse?.Invoke(arg);

        public static bool Then(this bool b, Action a) => b.IfTrue(a);

        public static T? Nullable<T>(this bool b, Func<T> f) where T : struct
        {
            return b ? f() : (T?) null;
        }

        public static bool OrIfTrue(this bool b1, bool b2, Action a)
        {
            if (b2) a();
            return b1 | b2;
        }
        public static T TriState<T>(this bool? b, T whenTrue, T whenFalse, T whenNull) => 
            b.HasValue ? b.Value ? whenTrue : whenFalse : whenNull;

        public static bool IsTrue(this bool? b) => b.HasValue && b.Value;
        public static bool All(this IEnumerable<bool> bs) => bs.All(b => b);
    }
}
