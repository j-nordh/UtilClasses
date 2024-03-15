using System;
using UtilClasses.Interfaces;

public static class CrudRepositoryExtensions
{
    public static bool MaybeGet<T>(this IRRepository<Guid, T> repo, Guid id, out T? res) where T : class, IHasGuid
    {
        try
        {
            res = repo.Get(id);
            return res != null;
        }
        catch
        {
            res = null;
            return false;
        }
    }
    public static bool MaybeGet<TKey, T>(this IRRepository<TKey, T> repo, TKey id, out T? res) where T : class
    {
        try
        {
            res = repo.Get(id);
            return res != null;
        }
        catch
        {
            res = null;
            return false;
        }
    }
}