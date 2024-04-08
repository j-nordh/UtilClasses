using System;
using System.Collections.Generic;

namespace UtilClasses;

public interface IDoubleDict<T1, T2> where T1 : IComparable
{
    List<(T1 a, T2 b)> State { get; }
    void Clear();
    T1 this[T2 v] { get; }
    T2 this[T1 v] { get; }
    IDoubleDict<T1,T2> Insert(T1 a, T2 b);
    IDoubleDict<T1,T2> Insert<TObj>(IEnumerable<TObj> lst, Func<TObj, T1>  a, Func<TObj, T2> b);
    T1 GetOrAdd(T2 val, Func<T1> f);
    T2 GetOrAdd(T1 val, Func<T2> f);
    bool TryGetValue(T1 val, out T2 ret );
    bool TryGetValue(T2 val, out T1 ret );
}