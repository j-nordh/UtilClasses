#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilClasses.Interfaces;

public interface IRRepository<in TKey, T>
{
    List<T> Get();
    T? Get(TKey id);
    void Refresh();
    bool TryGetValue(TKey key, out T val);
}

public interface IAsyncRRepository<in TKey, T>
{
    Task<List<T>> Get();
    Task<T?> Get(TKey id);
}

public interface ICruRepository< TKey, T> : IRRepository<TKey, T>
{
   TKey Add(T cfg);
    void Put(TKey id, T cfg);
}

public interface IAsyncCruRepository<TKey, T> : IAsyncRRepository<TKey, T>
{
    Task<TKey> Add(T o);
    Task Put(TKey id, T g);
}

public interface ICrudRepository<TKey, T> : ICruRepository<TKey, T>
{
    void Delete(TKey id);
}
public interface IAsyncCrudRepository<TKey, T> : IAsyncCruRepository<TKey, T>
{
    Task Delete(TKey id);
}