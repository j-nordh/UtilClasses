using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Dictionaries;

namespace UtilClasses.Cache
{   
    public class CachedKeywordStore
    {
        private readonly Func<string, int> _writeFunc;
        private readonly DictCache<string, object> _dict;
        private Dictionary<int, string> _reverse;
        public CachedKeywordStore(Func<IEnumerable<KeyValuePair<int, string>>> refreshFunc, Func<string, int> writeFunc,
            TimeSpan cacheTimeout, TimeSpan? minRefreshDelay)
        {
            _reverse = new Dictionary<int, string>();
            _writeFunc = writeFunc;
            _dict = new DictCache<string, object>(
                () =>
                {
                    var ret = refreshFunc()?.ToList();
                    if (ret == null) return null;
                    _reverse = ret.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    return ret.Select(kvp => new KeyValuePair<string, object>(kvp.Value.ToLower(), kvp.Key));
                },
                cacheTimeout,
                minRefreshDelay,
                false);
        }

        public int Get(string keyword)
        {
            var res = _dict.Get(keyword.ToLower());
            if (res != null) return (int)res;

            res = _writeFunc(keyword);
            _dict[keyword.ToLower()] = res;
            return (int)res;
        }

        public int this[string keyword] => Get(keyword);
        public string this[int i]
        {
            get
            {
                if (!_reverse.Any()) _dict.Update();
                return _reverse.Maybe(i);
            }
        }
    }
}
