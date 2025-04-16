using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UtilClasses.Core.Extensions.Funcs;

public static class FunctionExtensions
{
    public static void AsVoid<T>(this Func<T> f) => f();
    public static void AsVoid<T, T2>(this Func<T, T2> f, T x) => f(x);
    public static Action<T1,T2> AsVoid<T1, T2, T3>(this Func<T1, T2, T3> f) => (a,b)=>f(a,b);

    public static T InvokeOr<T>(this Func<T> f, Func<T> alt) => null != f ? f() : alt();

    public static Task InvokeOr<T>(this Func<Task> f, Func<T> alt) => null != f ? f() : Task.Run(alt);
    public static Task InvokeOr(this Func<Task> f, Action alt) => null != f ? f() : Task.Run(alt);

    public static Task InvokeOr<T>(this Func<T, Task> f, T o, Action alt) => null != f ? f(o) : Task.Run(alt);
    public static T InvokeOr<T, TIn>(this Func<TIn, T> f, TIn a, Func<T> alt) => null != f ? f(a) : alt();

    public static Func<T, T> Then<T>(this Func<T, T> a, Func<T, T> b) => x => b(a(x));
    public static Func<T, T> Chain<T>(params Func<T, T>[] fs) => o =>
    {
        foreach (var f in fs) o = f(o);
        return o;
    };

    public static void Run<T>(this IEnumerable<Action<T>> actions, T val)
    {
        foreach (var action in actions)
        {
            action(val);
        }
    }
    public static void OnEx(this Action a, Action<Exception> handler)
    {
        try
        {
            a();
        }
        catch (Exception ex)
        {
            handler(ex);
        }
    }

    public static bool All<T>(this IEnumerable<Func<T, bool>> fs, T val) => fs.All(f=> f(val));

    public static Func<T2, T3> Bind<T1, T2, T3>(T1 val, Func<T1, T2, T3> f) => a => f(val, a);
    public static Func<TRet> Bind<T1, TRet>(this Func<T1, TRet> f, T1 val) => () => f(val);
    public static Func<TRet> Bind<T1, T2, TRet>(this Func<T1, T2, TRet> f, T1 val, T2 val2) => () => f(val, val2);
    public static Func<TRet> Bind<T1, T2, T3, TRet>(this Func<T1, T2, T3, TRet> f, T1 val, T2 val2, T3 val3) => () => f(val, val2, val3);
    public static Func<T2, T3, T4> Bind<T1, T2, T3, T4>(T1 val, Func<T1, T2, T3, T4> f) => (a, b) => f(val, a,b);
    public static Action<T2> Bind<T1, T2>(T1 val, Action<T1, T2> f) => a => f(val, a);
    public static Action<T2, T3> Bind<T1, T2, T3>(T1 val, Action<T1, T2, T3> f) => (a,b) => f(val, a,b);
    public static Func<T1, T2, T3> FakeParameter<T1, T2, T3>(this Func<T1, T3> f) => (a, _) => f(a);
    public static Func<T1, T2, T3, TOut> FakeParameter<T1, T2, T3, TOut>(this Func<T1, TOut> f) => (a, _ ,__) => f(a);
    public static Func<T1, T2, T3, TOut> FakeParameter<T1, T2, T3, TOut>(this Func<T1, T2, TOut> f) => (a, b, _) => f(a, b);
    public static Func<T1, T2, T3, T4, TOut> FakeParameter<T1, T2, T3, T4, TOut>(this Func<TOut> f) => (_, __, ___, ____) => f();
    public static Func<T1, T2, T3, T4, TOut> FakeParameter<T1, T2, T3, T4, TOut>(this Func<T1, TOut> f) => (a, _, __, ___) => f(a);
    public static Func<T1, T2, T3, T4, TOut> FakeParameter<T1, T2, T3, T4, TOut>(this Func<T1, T2, TOut> f) => (a, b, _,__) => f(a, b);
    public static Func<T1, T2, T3, T4, TOut> FakeParameter<T1, T2, T3, T4, TOut>(this Func<T1, T2, T3, TOut> f) => (a, b, c, __) => f(a, b, c);
    public static Func<T1, T2> FakeParameter<T1, T2>(this Func<T2> f) => (_) => f();
}