using System;
using System.Collections.Generic;
using System.Text;

namespace UtilClasses.Extensions.Guids
{
    public static class GuidExtensions
    {
        public static Guid AsGuid(this string s) => Guid.Parse(s);
        public static Guid AsGuid(this object o) => Guid.Parse(o.ToString());

    }
}
