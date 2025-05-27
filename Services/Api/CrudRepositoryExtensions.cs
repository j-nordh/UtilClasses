using System;
using System.Threading.Tasks;
using UtilClasses.Interfaces;

public static class CrudRepositoryExtensions
{
    public static bool MaybeGet<T>(this IRepository<Guid, T> repo, Guid id, out T? res) where T : class, IHasGuid
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

    public class Maybe<T>
    {
        public bool Result { get; set; }
        public T? Value { get; set; }

        public Maybe() { }
        public Maybe(bool result, T? value)
        {
            Result = result;
            Value = value;
        }
        public static implicit operator bool(Maybe<T> m) => m.Result;
        public static implicit operator T?(Maybe<T> m) => m.Value;

        
    }

    public static class Maybe
    {
        public static Maybe<T> NotNull<T>(T? o) => new(o != null, o);
    }

    public static bool MaybeGet<TKey, T>(this IRepository<TKey, T> repo, TKey id, out T? res) where T : class
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