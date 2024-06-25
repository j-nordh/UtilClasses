using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Reflections;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Types;
using UtilClasses.Interfaces;

namespace UtilClasses;

public class TypeLookup<TAnchor> : ITypeLookup
{
    private DictionaryOic<Type> _lookup = new();
    
    private static readonly Dictionary<Type, Func<object, object>> _idExtractors = new();

    public List<(string name, Func<Type, bool> f)> Predicates { get; } = new();

    public List<Func<Type, string?>> NameFetchers { get; } = new();

    private TypeLookup<TAnchor> Add(Func<Type, bool> f, [CallerMemberName] string? caller = null)
    {
        caller = caller?.RemoveAllOic("Filter_") ?? "Unknown";
        Predicates.Add((caller, f));
        return this;
    }

    public TypeLookup<TAnchor> Filter_OnFullName(Func<string, bool> predicate) => Add(t => predicate(t.FullName!));

    private TypeLookup<TAnchor> Add(Func<Type, string> f)
    {
        NameFetchers.Add(f);
        return this;
    }

    public TypeLookup<TAnchor> Naming_UseTypeName() => Add(t => t.Name);
    public TypeLookup<TAnchor> Naming_UseFullName() => Add(t => t.FullName);
    public TypeLookup<TAnchor> Naming_UseSaneName() => Add(t => t.SaneName());
    public TypeLookup<TAnchor> Naming_Use(Func<Type, string?> f) => Add(f);

    public TypeLookup<TAnchor> Filter_IsClass(bool b = true) => Add(t => t.IsClass == b);
    public TypeLookup<TAnchor> Filter_IsInterface(bool b = true) => Add(t => t.IsInterface == b);
    public TypeLookup<TAnchor> Filter_IsAbstract(bool b = true) => Add(t => t.IsAbstract == b);
    public TypeLookup<TAnchor> Filter_IsGeneric(bool b = true) => Add(t => t.IsGenericType == b);
    public TypeLookup<TAnchor> Filter_IsStatic(bool b = true) => Add(t => (t.IsAbstract && t.IsSealed) == b);
    public TypeLookup<TAnchor> Filter_IsNested(bool b = true) => Add(t => t.IsNested == b);
    public TypeLookup<TAnchor> Filter_Inherits<TBase>() => Add(t => t.BaseType == typeof(TBase));
    public TypeLookup<TAnchor> Filter_Implements<TInterface>() => Add(t => t.GetInterfaces().Contains(typeof(TInterface)));
    public TypeLookup<TAnchor> Filter_HasAttribute<TAttr>() => Add(t => t.GetFirstCustomAttribute<TAttr>() != null);

    public void Init()
    {
        DictionaryOic<int> rejectCounters = new();
        var ts = typeof(TAnchor).Assembly.DefinedTypes.Cast<Type>();
        foreach (var t in ts)
        {
            var ok = true;
            foreach (var pred in Predicates.Where(pred => !pred.f(t)))
            {
                ok = false;
                rejectCounters.Increment(pred.name);
            }

            if (!ok)
                continue;
            var names = NameFetchers.Select(f => f(t)).NotNull().DistinctOic();
            names.ForEach(n => _lookup.Add(n, t));
            
            var idAdded = AddId<IHasIntId>(t, o => o.Id) || AddId<IHasGuid>(t, o => o.Id);
        }
    }
    
    public TId Id<TObj, TId>(TObj obj)
    {
        if (!_idExtractors.TryGetValue(typeof(TObj), out var f))
            throw new KeyNotFoundException($"Could not find an Id extractor for {typeof(TObj).SaneName()}");
        var id = f(obj!);
        if (id.GetType() != typeof(TId))
            throw new Exception(
                $"The extracted id is a {id.GetType().SaneName()} which does not match the requested {typeof(TId).SaneName()}.");
        return (TId)id;
    }

    public bool AddId<T>(Func<T, object> f) => AddId(typeof(T), f);

    public bool AddId<T>(Type t, Func<T, object> f)
    {
        if (!t.CanBe<T>()) return false;
        _idExtractors.Add(t, o => f((T)o));
        return true;
    }

    public IEnumerable<string> Names => _lookup.Keys;
    public Type? Lookup(string key) => _lookup.Maybe(key);
    public IEnumerable<Type> Types() => _lookup.Values.Distinct();

    public Type? this[string name] => _lookup.Maybe(name);
}