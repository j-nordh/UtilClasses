using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Cache
{
    public class StubDictCache<TId, T> : DictCache<TId, T> where T : class
    {
        private readonly List<T> _vals;

        public StubDictCache(IEnumerable<T> vals, Func<T, TId> idExtractor) : base(() => new T[] { }, idExtractor, TimeSpan.MaxValue,null, false)
        {
            _vals = vals.ToList();
            _objectRefreshFunc = () => _vals;
        }
    }
}
