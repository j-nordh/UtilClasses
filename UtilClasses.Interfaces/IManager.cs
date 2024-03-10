using System;

namespace UtilClasses.Interfaces
{
    public interface IManager<T, TConfig> : ICrudRepository<TConfig>, IManager where TConfig : class, IHasGuid
    {
        T Fetch(Guid id);
    }

    public interface IManager
    {
    }
}