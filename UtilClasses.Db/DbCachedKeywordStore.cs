using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using UtilClasses.Cache;

namespace UtilClasses.Db
{
    public class DbCachedKeywordStore : IKeywordStore
    {
        private readonly ConnectionManager _conMan;
        private CachedKeywordStore _dict;

        public DbCachedKeywordStore(ConnectionManager conMan,  Func<SqlConnection, IEnumerable<KeyValuePair<int, string>>> refreshFunc,
            Func<SqlConnection, string, int> writeFunc, TimeSpan cacheTimeout, TimeSpan? minRefreshDelay)
        {
            _conMan = conMan;
            _dict = new CachedKeywordStore(() => _conMan.Do(refreshFunc), s => _conMan.Do(c=>writeFunc(c,s)),
                cacheTimeout, minRefreshDelay);
        }

        public int Get(string keyword) => _dict.Get(keyword);
        public int this[string keyword] => Get(keyword);
        public string this[int i] => _dict[i];
    }
}
