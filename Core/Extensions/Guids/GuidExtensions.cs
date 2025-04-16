using System;

namespace UtilClasses.Core.Extensions.Guids;

public static class GuidExtensions
{
    public static Guid AsGuid(this string s) => Guid.Parse(s);
    public static Guid AsGuid(this object o) => Guid.Parse(o.ToString());

}