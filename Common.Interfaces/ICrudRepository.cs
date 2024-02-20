using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ICrudRepository<T> : ICruRepository<T> where T : class, IHasGuid
    {
        void Delete(Guid id);
        void Clear();
    }

    public interface IRRepository<T> :IRRepository<Guid, T> where T:IHasGuid 
    {
    }
    public interface IRRepository<in TKey, T>
    {
        List<T> Get();
        T Get(TKey id);
        void Refresh();
    }
    public interface IAsyncRRepository<T>
    {
        Task<List<T>> Get();
        Task<T> Get(Guid id);
    }
    public interface ICruRepository<T> : IRRepository<T> where T:IHasGuid
    {
        T Add(T cfg);
        void Put(Guid id, T cfg);
    }
}
