using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilClasses.Extensions.Booleans;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Interfaces;

namespace UtilClasses.Cache
{
    public static class DictCache
    {
        public static DictCache<T> Create<T>(Func<IEnumerable<T>> refresh) where T : class, IHasLongId => new DictCache<T>(refresh);
        public static DictCache<T> Create<T>(long i,Func<IEnumerable<T>> refresh, Func<long, IEnumerable<T>> idRefresh) where T : class, IHasLongId =>
            i<=0 
                ? new DictCache<T>(refresh) 
                : new DictCache<T>(()=>idRefresh(i));



        public static DictCache<TId, T> Create<TId, T>(Func<IEnumerable<T>> refresh, Func<T, TId> idExtractor) where T : class 
            => new DictCache<TId, T>(refresh, idExtractor);
        public static DictCache<TId,T> Create<TId, T>(long i, Func<IEnumerable<T>> refresh, Func<long, IEnumerable<T>> idRefresh, Func<T, TId> idExtractor) where T : class, IHasLongId =>
            i <= 0
                ? new DictCache<TId, T>(refresh, idExtractor)
                : new DictCache<TId,T>(() => idRefresh(i), idExtractor);
    }

    public class DictCache<THasId> : DictCache<long, THasId> where THasId : class, IHasLongId
    {
        public DictCache(Func<IEnumerable<THasId>> refresh):base(refresh, o => o.Id)
        { }
    }
    public class DictCache<TId, T> :DictCacherBase<TId,T> where T:class 
    {
        protected Func<IEnumerable<T>>? _objectRefreshFunc;
        protected Func<T, TId>? _idExtractor;
        protected Func<IEnumerable<KeyValuePair<TId, T>>>? _kvpRefreshFunc;

        public DictCache(Func<IEnumerable<T>> refreshFunc, Func<T, TId> idExtractor, bool updateOnMiss=true) : this(refreshFunc, idExtractor, TimeSpan.MaxValue, null, updateOnMiss )
        {}

        public DictCache(Func<IEnumerable<KeyValuePair<TId, T>>> refreshFunc, TimeSpan cacheTimeout, TimeSpan? minRefreshDelay,
            bool updateOnMiss) : base(cacheTimeout,minRefreshDelay, updateOnMiss)
        {
            _kvpRefreshFunc = refreshFunc;
        }


        public DictCache(Func<IEnumerable<T>> refreshFunc, Func<T, TId> idExtractor, TimeSpan cacheTimeout, TimeSpan? minRefreshDelay,
            bool updateOnMiss) : base(cacheTimeout,minRefreshDelay, updateOnMiss)
        {
            _objectRefreshFunc = refreshFunc;
            _idExtractor = idExtractor;
        }

        public T? FirstOrDefault(Func<T, bool> pred)
        {
            UpdateIfNeeded();
            return _dict.Values.FirstOrDefault(pred);
        }

        public void Update()
        {
            try
            {
                _dict.Clear();
                _objectRefreshFunc?.Invoke()?.ForEach(item => _dict[_idExtractor(item)] = item);
                _kvpRefreshFunc?.Invoke()?.ForEach(kvp => _dict[kvp.Key] = kvp.Value);
                _fetched = DateTime.Now;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public virtual T? Get(TId id, T? def = null) => Get(id, () => def);

        public virtual T? Get(TId id, Func<T?> fDef)
        {
            if (null == _idExtractor)
                throw new Exception("The IdExtractor has not been configured!");
            T? GetDefault()
            {
                var r = fDef();
                if (null == r) return null;

                _dict[_idExtractor(r)] = r;
                return r;
            }
            UpdateIfNeeded();
            if (_dict.ContainsKey(id)) return _dict[id];
            if (!_updateOnMiss) return GetDefault();
            if (_blackList.ContainsKey(id) && DateTime.Now < _blackList[id] + BlackListTimeout) return GetDefault();
            Update();
            if (_dict.ContainsKey(id)) return _dict[id];
            var ret = GetDefault();
            if (null != ret) return ret;
            _blackList[id] = DateTime.Now;
            return null;
        }
        public List<T>All()
        {
            UpdateIfNeeded();
            return _dict.Values.ToList();
        }

        public List<T> All(Func<T, bool> predicate) => All().Where(predicate).ToList();


        public T this[TId id]
        {
            get { return Get(id); }
            set { _dict[id] = value; }
        }

        protected void UpdateIfNeeded() => UpdateNeeded().IfTrue(Update);
    }

    public class AsyncDictCacher<T> : AsyncDictCacher<Guid, T> where T : class, IHasGuid
    {
        public AsyncDictCacher(TimeSpan cacheTimeout, TimeSpan? minRefreshDelay, bool updateOnMiss, Func<Task<List<T>>> updateFunc) : base(cacheTimeout, minRefreshDelay, updateOnMiss, updateFunc, o=>o.Id)
        {
        }
        public AsyncDictCacher(CacherParameters ps, Func<Task<List<T>>> updateFunc): base(ps, updateFunc, o=>o.Id){}
    }
    public class AsyncDictCacher<TId, T> : DictCacherBase<TId, T> where T : class
    {
        private readonly Func<Task<IEnumerable<KeyValuePair<TId, T>>>> _updateFunc;
        public AsyncDictCacher(TimeSpan cacheTimeout, TimeSpan? minRefreshDelay, bool updateOnMiss, Func<Task<IEnumerable<KeyValuePair<TId, T>>>> updateFunc) : base(cacheTimeout, minRefreshDelay, updateOnMiss)
        {
            _updateFunc = updateFunc;
        }

        public AsyncDictCacher(TimeSpan cacheTimeout, TimeSpan? minRefreshDelay, bool updateOnMiss, Func<Task<List<T>>> updateFunc,
            Func<T, TId> idExtractor) : this(cacheTimeout, minRefreshDelay, updateOnMiss,
            async () => (await updateFunc())?.Select(o => new KeyValuePair<TId, T>(idExtractor(o), o))??new List<KeyValuePair<TId, T>>())
        { }

        public AsyncDictCacher(CacherParameters ps, Func<Task<IEnumerable<KeyValuePair<TId, T>>>> updateFunc) : base(ps)
        {
            _updateFunc = updateFunc;
        }

        public AsyncDictCacher(CacherParameters ps, Func<Task<List<T>>> updateFunc, Func<T, TId> idExtractor) :base(ps)
        {
            _updateFunc = async () =>
            {
                var objects = await updateFunc();
                var ret = new List<KeyValuePair<TId, T>>();
                foreach (var o in objects)
                {
                    ret.Add(new KeyValuePair<TId, T>(idExtractor(o), o));
                }

                return ret;
            };
        }

        public async Task Update()
        {
            _dict.Clear();
            var newValues = await _updateFunc();
            newValues.ForEach(kvp=>_dict[kvp.Key] = kvp.Value);
            _fetched = DateTime.Now;
        }
        protected async Task UpdateIfNeeded()
        {
            if (UpdateNeeded()) await Update();
        }
        public T this[TId id]
        {
            set { _dict[id] = value; }
        }
        public async Task<T> Get(TId id)
        {
            await UpdateIfNeeded();
            if (_dict.ContainsKey(id)) return _dict[id];
            if (!_updateOnMiss) return null;
            if (_blackList.ContainsKey(id) && DateTime.Now < _blackList[id] + BlackListTimeout) return null;
            await Update();
            if (_dict.ContainsKey(id)) return _dict[id];
            _blackList[id] = DateTime.Now;
            return null;
        }

        public async Task<List<T>> All()
        {
            await UpdateIfNeeded();
            return _dict.Values.ToList();
        }
    }

    public class CacherParameters
    {
        public TimeSpan Timeout { get; set; } =TimeSpan.FromMinutes(2);
        public TimeSpan MinRefreshDelay { get; set; } = TimeSpan.FromMilliseconds(500);
        public bool UpdateOnMiss { get; set; }
    }
    public class WriteThroughCacherParameters<T>: CacherParameters where T:IHasGuid
    {
        public  Func<Task<List<T>>> Get { get; set; }
        public  Func<Guid, Task> Delete { get; set; }
        public  Func<T, Task<T>> Save { get; set; } 
        public Func<List<T>, Task<List<T>>> SaveAll { get; set; }
        public Func<Task> Clear { get; set; }
        public Func<T, Guid> IdExtractor { get; set; } = o => o.Id;
    }

    public class AsyncWriteThroughCacher<T> :AsyncDictCacher<Guid, T>where T:class, IHasGuid
    {
        readonly WriteThroughCacherParameters<T> _ps;
        public AsyncWriteThroughCacher(WriteThroughCacherParameters<T> ps) : base(ps, ps.Get, ps.IdExtractor)
        {
            _ps = ps;
        }

        public async Task Delete(T obj)
        {
            var id = _ps.IdExtractor(obj);
            await _ps.Delete(id);
            _dict.Remove(id);
        }
        public async Task Delete(Guid id)
        {
            await _ps.Delete(id);
            _dict.Remove(id);
        }


        public async Task Save(T obj)
        {
            obj = await _ps.Save(obj);
            var id = _ps.IdExtractor(obj);
            _dict[id] = obj;
        }

        public async Task Save(List<T> items)
        {
            var saved =await _ps.SaveAll(items);
            foreach(var item in saved)
            {
                _dict[_ps.IdExtractor(item)] = item;
            }
        }

        public async Task Clear()
        {
            await _ps.Clear();
            _dict.Clear();
        }
    }
    public abstract class DictCacherBase<TId, T> : Cacher where T:class 
    {
        protected readonly Dictionary<TId, T> _dict;
        protected readonly Dictionary<TId, DateTime> _blackList;
        protected readonly bool _updateOnMiss;
        public TimeSpan BlackListTimeout { get; set; }
        protected DictCacherBase(TimeSpan cacheTimeout, TimeSpan? minRefreshDelay, bool updateOnMiss) : base(cacheTimeout, minRefreshDelay)
        {
            _updateOnMiss = updateOnMiss;
            _dict = new Dictionary<TId, T>();
            _blackList = new Dictionary<TId, DateTime>();
            BlackListTimeout = TimeSpan.FromSeconds(10);
        }
        protected DictCacherBase(CacherParameters ps): this(ps.Timeout, ps.MinRefreshDelay, ps.UpdateOnMiss){}
        public bool ContainsNow(TId id) => _dict.ContainsKey(id);

        public T GetNow(TId id) => _dict.ContainsKey(id) ? _dict[id] : null;


        public IReadOnlyDictionary<TId, T> AsDictionary() => _dict;
        public List<T> AllNow() => _dict.Values.ToList();
        

        public override void Invalidate()
        {
            base.Invalidate();
            _dict.Clear();
        }
    }
}