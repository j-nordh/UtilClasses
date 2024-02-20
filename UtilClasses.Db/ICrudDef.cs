using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Db
{
    public interface ICrudRepo<T>
    {
        T Create(T obj);
        T Get(long id);
        List<T> All();
        void Update(T obj);
        void Delete(long id);
    }

    //public interface ILinkRepo<T,TChild> where T:IHasLinked<TChild>
    //{
    //    void Clear(T parent);
    //    void Clear(TChild child);
    //    void Set(T parent, TChild child);
    //    List<TChild> GetChildren(long parentId);
    //    List<T> GetParents(long childId);
    //    void Load(T parent);
    //    void Save(T parent);
    //}

    //public interface IHasLinked<T>
    //{
    //    List<T> GetLinked();
    //    void SetLinked(IEnumerable<T> objs);
    //}

    //class Test : IHasLinked<ConsoleTable>
    //{
    //    public List<T> GetLinked<T>()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetLinked(IEnumerable<ConsoleTable> objs)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public interface IUpsertRepo<T>
    {
        T Upsert(T obj);
    }

    public interface IMultiUpsertRepo<T>:IUpsertRepo<T>
    {
        List<T> Upsert(IEnumerable<T> items);
    }

    public static class UpsertRepoExtensions
    {
        public static Task UpsertAsync<T>(this IUpsertRepo<T> repo, T obj) => Task.Run(() => repo.Upsert(obj));

        public static Task UpsertAsync<T>(this IMultiUpsertRepo<T> repo, IEnumerable<T> os) =>
            Task.Run(() => repo.Upsert(os));

        public static List<T> Upsert<T, T2>(this IMultiUpsertRepo<T> repo, IEnumerable<T2> parents,
            Func<T2, List<T>> selector) => repo.Upsert(parents.SelectMany(selector));

    }
    

}
