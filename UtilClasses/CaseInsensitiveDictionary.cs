using System;
using System.Collections.Generic;

namespace UtilClasses
{
    [Obsolete("Use DictionaryOic instead")]
    public class CaseInsensitiveDictionary<T>:Dictionary<string,T> 
    {
       public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase)
        { }
    }

    public class DictionaryOic<T> : Dictionary<string, T>
    {
        public DictionaryOic() : base(StringComparer.OrdinalIgnoreCase)
        {}
    }
}
