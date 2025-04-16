using System.Collections.Specialized;
using System.Linq;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Extensions.NameValueCollections;

public static class NameValueCollectionExtensions
{
    public static string Maybe(this NameValueCollection col, string key)
    {
        var k = col.AllKeys.FirstOrDefault(key.EqualsOic);
        return null == k 
            ? null 
            : col[k];
    }
}