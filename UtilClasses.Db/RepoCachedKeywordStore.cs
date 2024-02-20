using System;
using System.Collections.Generic;
using UtilClasses.Cache;

namespace UtilClasses.Db
{
    public class RepoCachedKeywordStore<T> : IKeywordStore
    {
        private readonly T _repo;
        private CachedKeywordStore _dict;
        public RepoCachedKeywordStore(T repo,  Func<T, IEnumerable<KeyValuePair<int, string>>> refreshFunc, Func<T, string, int> writeFunc, TimeSpan cacheTimeout, TimeSpan? minRefreshDelay)
        {
            _repo = repo;
            _dict = new CachedKeywordStore(()=>refreshFunc(_repo), s=>writeFunc(_repo,s), cacheTimeout, minRefreshDelay);
        }

        public int this[string keyword] => Get(keyword);

        public string this[int i] => _dict[i];

        public int Get(string keyword)=>_dict.Get(keyword);
    }
}