using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilClasses.Interfaces;

public interface IRRepository<in TKey, T>
{
    List<T> Get();
    T Get(TKey id);
    void Refresh();
    bool TryGetValue(TKey key, out T val);
}
public interface ICruRepository<in TKey, T> : IRRepository<TKey, T> 
{
    T Add(T cfg);
    void Put(TKey id, T cfg);
}
public interface ICrudRepository<in TKey, T> : ICruRepository<TKey, T> 
{
    void Delete(Guid id);
    void Clear();
}
#region Integer-keyed repos



public interface IIntRRepository<T> : IRRepository<int, T> where T : IHasIntId
{
}

public interface IIntCruRepository<T> : ICruRepository<int, T>, IIntRRepository<T> where T : IHasIntId
{}
public interface IIntCrudRepository<T>: ICrudRepository<int, T>, IIntCruRepository<T> where T : class, IHasIntId
{}
#endregion

#region Guid-keyed repos
public interface IRRepository<T> : IRRepository<Guid, T> where T : IHasGuid
{
}
public interface ICruRepository<T> : ICruRepository<Guid, T>, IRRepository<T> where T : IHasGuid
{}



public interface ICrudRepository<T> : ICrudRepository<Guid, T>, ICruRepository<T> where T : class, IHasGuid
{ }

#endregion

public interface IAsyncRRepository<T>
{
    Task<List<T>> Get();
    Task<T> Get(Guid id);
}