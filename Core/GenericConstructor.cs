using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UtilClasses.Core.Extensions.Dictionaries;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Funcs;
using UtilClasses.Core.Extensions.Types;

namespace UtilClasses.Core;

public class GenericConstructor<T, TKey, T1> : GenericConstructor<T, TKey, T1, object, object, object> where T : class
{
    public GenericConstructor(IEqualityComparer<TKey> cmp) : base(cmp)
    {
    }

    public T Construct(TKey key, T1 arg1) => Construct(key, arg1, null, null, null);
    public List<T> ConstructAll(TKey key, T1 arg1) => ConstructAll(key, arg1, null, null, null);
    public List<T> ConstructAll(TKey key, Func<Type, bool> predicate, T1 arg1) =>
        ConstructAll(key, predicate, arg1, null, null, null);
    public new GenericConstructor<T, TKey, T1> LoadAssembly(Assembly ass, Func<Type, TKey> keyExtractor) =>
        LoadAssembly(ass, keyExtractor, _ => true);
    public new GenericConstructor<T, TKey, T1> LoadAssembly(Assembly ass, Func<Type, TKey> keyExtractor, Func<Type, bool> predicate) => 
        (GenericConstructor<T, TKey, T1>)base.LoadAssembly(ass, keyExtractor, predicate);
}
public class GenericConstructor<T, TKey, T1, T2> : GenericConstructor<T, TKey, T1, T2, object, object> where T : class
{
    public GenericConstructor(IEqualityComparer<TKey> cmp) : base(cmp)
    {
    }

    public T? Construct(TKey key, T1 arg1, T2 arg2) => Construct(key, arg1, arg2, null, null);
    public List<T> ConstructAll(TKey key, T1 arg1, T2 arg2) => ConstructAll(key, arg1, arg2, null, null);
    public List<T> ConstructAll(TKey key, Func<Type, bool> predicate, T1 arg1, T2 arg2) =>
        ConstructAll(key, predicate, arg1, arg2, null, null);
    public new GenericConstructor<T, TKey, T1, T2> LoadAssembly(Assembly ass, Func<Type, TKey> keyExtractor) =>
        LoadAssembly(ass, keyExtractor, _ => true);
    public new GenericConstructor<T, TKey, T1, T2> LoadAssembly(Assembly ass, Func<Type, TKey> keyExtractor, Func<Type, bool> predicate) => 
        (GenericConstructor<T, TKey, T1, T2>)base.LoadAssembly(ass, keyExtractor, predicate);
}
public class GenericConstructor<T, TKey, T1, T2, T3> : GenericConstructor<T, TKey, T1, T2, T3, object> where T : class
{
    public GenericConstructor(IEqualityComparer<TKey> cmp) : base(cmp)
    {
    }

    public T? Construct(TKey key, T1 arg1, T2 arg2, T3 arg3) => Construct(key, arg1, arg2, arg3, default);
    public List<T> ConstructAll(TKey key, T1 arg1, T2 arg2, T3 arg3) => ConstructAll(key, arg1, arg2, arg3, null);

    public List<T> ConstructAll(TKey key, Func<Type, bool> predicate, T1 arg1, T2 arg2, T3 arg3) =>
        ConstructAll(key, predicate, arg1, arg2, arg3, null);

    public new GenericConstructor<T, TKey, T1, T2, T3> LoadAssembly(Assembly ass, Func<Type, TKey> keyExtractor) =>
        LoadAssembly(ass, keyExtractor, _ => true);
    public new GenericConstructor<T, TKey, T1, T2, T3> LoadAssembly(Assembly ass, Func<Type, TKey> keyExtractor, Func<Type, bool> predicate) => 
        (GenericConstructor<T, TKey, T1, T2, T3>)base.LoadAssembly(ass, keyExtractor, predicate);
}

public class GenericStringConstructor<T, T1, T2, T3, T4> : GenericConstructor<T, string, T1, T2, T3, T4> where T : class 
{
    public GenericStringConstructor(StringComparer? cmp=null) : base(cmp??StringComparer.OrdinalIgnoreCase)
    {
    }
}
public class GenericStringConstructor<T, T1, T2, T3> : GenericConstructor<T, string, T1, T2, T3> where T : class
{
    public GenericStringConstructor(StringComparer? cmp = null) : base(cmp ?? StringComparer.OrdinalIgnoreCase)
    {
    }
}
public class GenericStringConstructor<T, T1, T2> : GenericConstructor<T, string, T1, T2> where T : class
{
    public GenericStringConstructor(StringComparer? cmp = null) : base(cmp ?? StringComparer.OrdinalIgnoreCase)
    {
    }
}
public class GenericStringConstructor<T, T1> : GenericConstructor<T, string, T1> where T : class
{
    public GenericStringConstructor(StringComparer? cmp = null) : base(cmp ?? StringComparer.OrdinalIgnoreCase)
    {
    }
}
public class GenericConstructor<T, TKey, T1, T2, T3, T4> where T : class
{
    private readonly Dictionary<TKey, List<(Type type, Func<T1?, T2?, T3?, T4?, T> constructor)>> _constructors;

    public GenericConstructor(IEqualityComparer<TKey> cmp)
    {
        _constructors = new (cmp);
    }

    public T? Construct(TKey key, T1 arg1, T2? arg2, T3? arg3, T4? arg4)
    {
        var cs = _constructors.Maybe(key);
        return cs.IsNullOrEmpty() ? null : cs.Single().constructor(arg1, arg2, arg3, arg4);
    }

    public List<T> ConstructAll(TKey key, T1? arg1, T2? arg2, T3? arg3, T4? arg4) =>
        ConstructAll(key, _ => true, arg1, arg2, arg3, arg4);
    public List<T> ConstructAll(TKey key, Func<Type, bool> predicate, T1? arg1, T2? arg2, T3? arg3, T4? arg4)
    {
        var cs = _constructors.Maybe(key);
        if (cs.IsNullOrEmpty()) return new List<T>();
        return cs
            .Where(i=>predicate(i.type))
            .Select(i => i.constructor(arg1, arg2, arg3, arg4))
            .ToList();
    }
    public List<T> ConstructAll(Func<Type, bool> predicate, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        var all = _constructors.Values.ToList();
        if (all.IsNullOrEmpty()) return new List<T>();

        return all.SelectMany(t=>t)
            .Distinct(i=>i.type)
            .Where(i => predicate(i.type))
            .Select(i => i.constructor(arg1, arg2, arg3, arg4))
            .ToList();
    }

    public bool Contains(TKey key) => _constructors.ContainsKey(key);

    public  GenericConstructor<T, TKey, T1, T2, T3, T4> LoadAssembly(Assembly ass, Func<Type, TKey> keyExtractor) =>
        LoadAssembly(ass, keyExtractor, _ => true);
    public  GenericConstructor<T, TKey, T1, T2, T3, T4> LoadAssembly(Assembly ass, Func<Type, TKey> keyExtractor, Func< Type, bool> predicate)
    {
        var ts = ass.DefinedTypes.Implementing<T>().RequireNotAbstract();
        foreach (var t in ts)
        {
            if(!predicate(t)) continue;
            var key = keyExtractor(t);

            if (t.HasConstructor()) AddConstructor(key, t, t.GenerateConstructor<T>());
            if (t.HasConstructor<T1>()) AddConstructor(key, t, t.GenerateConstructor<T1?, T>());
            if (t.HasConstructor<T1, T2>()) AddConstructor(key, t, t.GenerateConstructor<T1?, T2?, T>());
            if (t.HasConstructor<T1, T2, T3>()) AddConstructor(key, t, t.GenerateConstructor<T1?, T2?, T3?, T>());
            if (t.HasConstructor<T1, T2, T3, T4>()) AddConstructor(key, t, t.GenerateConstructor<T1?, T2?, T3?, T4?, T>());
        }

        return this;
    }

    private void AddConstructor(TKey key, Type t, Func<T1?, T2?, T3?, T4?, T> c)
    {
        var lst = _constructors.GetOrAdd(key);
        lst.Add((t,c));
    }

    private void AddConstructor(TKey key, Type t, Func<T1?, T2?, T3?, T> c) =>
        AddConstructor(key, t, c.FakeParameter<T1?, T2?, T3?, T4?, T>());

    private void AddConstructor(TKey key, Type t, Func<T1?, T2?, T> c) =>
        AddConstructor(key, t, c.FakeParameter<T1?, T2?, T3?, T4?, T>());

    private void AddConstructor(TKey key, Type t, Func<T1?, T> c) =>
        AddConstructor(key, t, c.FakeParameter<T1?, T2?, T3?, T4?, T>());

    private void AddConstructor(TKey key, Type t, Func<T> c) =>
        AddConstructor(key, t, c.FakeParameter<T1?, T2?, T3?, T4?, T>());

}