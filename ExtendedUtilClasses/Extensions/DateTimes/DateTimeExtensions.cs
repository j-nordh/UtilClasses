using System;
using System.Collections.Generic;
using System.Text;

namespace ExtendedUtilClasses.Extensions.DateTimes;

public static class DateTimeExtensions
{
    public static string ToSaneString(this DateOnly dt) => dt.ToString("yyyy-MM-dd");
    public static string ToSaneString(this TimeOnly dt) => dt.ToString("T");
}
