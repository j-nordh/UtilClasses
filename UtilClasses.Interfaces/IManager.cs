using System;

namespace UtilClasses.Interfaces
{
    public interface IManager<TKey, T> : ICrudRepository<TKey, T>, IManager
    {
    }

    public interface IManager
    {
    }
}