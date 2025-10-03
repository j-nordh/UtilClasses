#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilClasses.Interfaces;

public interface IAsyncRepository
{
    Task Refresh();
};

public interface IAsyncRRepository<T> : IAsyncRepository
{
    Task<List<T>> GetAsync();
    Task<int> Count();
}

public interface IAsyncRRepository<in TKey, T> : IAsyncRRepository<T>
{
    Task<T?> GetAsync(TKey id);
    Task<List<T>> GetAsync(IEnumerable<TKey> ids);
}

public interface IAsyncCruRepository<TKey, T> : IAsyncRRepository<TKey, T>
{
    event Action<T> Added;
    event Action<T> Updated;
    Task<TKey> Add(T o);
    Task<List<TKey>> Add(IEnumerable<T> o);
    Task<bool> Put(TKey id, T g);
}

public interface IAsyncCrudRepository<TKey, T> : IAsyncCruRepository<TKey, T>
{
    event Action<TKey> Deleted;
    Task<bool> Delete(TKey id);

}