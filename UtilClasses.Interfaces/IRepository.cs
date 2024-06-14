#nullable enable
using System.Collections.Generic;

namespace UtilClasses.Interfaces;

public interface IRepository
{
    void Refresh();
}

public interface IRepository<T> :IRepository
{
    List<T> Get();
}
public interface IRepository<in TKey, T> : IRepository<T>
{
    T? Get(TKey id);

    bool TryGetValue(TKey key, out T val);
}
public interface ICruRepository<TKey, T> : IRepository<TKey, T>
{
    TKey Add(T cfg);
    void Put(TKey id, T cfg);
}

public interface ICrudRepository<TKey, T> : ICruRepository<TKey, T>
{
    void Delete(TKey id);
}