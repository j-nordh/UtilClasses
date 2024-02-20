using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Extensions.NameValueCollections
{
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
}
