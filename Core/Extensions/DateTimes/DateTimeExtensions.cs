using System;
using System.Globalization;
using System.Linq;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Extensions.DateTimes;

public static class DateTimeExtensions
{
    private static DateTime _epoch = new DateTime(1970, 1, 1);
    public static bool AfterAll(this DateTime dt, params DateTime[] others)
    {
        return others.All(d => dt > d);
    }

    public static long ToUnixTime(this DateTime dt) => (dt - _epoch).Ticks/TimeSpan.TicksPerMillisecond;
    public static DateTime ToDateTime(this long unixTimeStamp) => _epoch + TimeSpan.FromMilliseconds(unixTimeStamp);

    public static bool Ish(this DateTime a, DateTime b, int maxDiffMs = 1000) =>
        Math.Abs((a - b).TotalMilliseconds) < maxDiffMs;

    public static string ToSaneString(this DateTime dt) => dt.ToString("s").Replace("T", " ");
    public static string ToSaneString(this DateTime? dt) => dt.HasValue
        ?  dt.Value.ToString("s").Replace("T", " ")
        : null;
    public static string ToFileString(this DateTime dt) => dt.ToSaneString().RemoveAllOic("-", ":").Replace(" ", "_").Substring(0, 15);
    public static int DaysInMonth(this DateTime dt) => DateTime.DaysInMonth(dt.Year, dt.Month);
    public static DateTime WithKind(this DateTime dt, DateTimeKind kind) => DateTime.SpecifyKind(dt, kind);
    public static (DateTime from, DateTime to) GetMonthBracket(this DateTime dt) 
        =>(new DateTime(dt.Year, dt.Month, 1), new DateTime(dt.Year, dt.Month, dt.DaysInMonth()));

    public static DateTime ToStartOfSecond(this DateTime dt)=> dt.Millisecond == 0
        ? dt
        : new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind);

    public static DateTime? ToStartOfSecond(this DateTime? dt) => dt?.ToStartOfSecond();
    public static DateTime ToStartOfMinute(this DateTime dt) => dt.Millisecond==0 && dt.Second ==0?dt
        : new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute,0, dt.Kind);
    public static DateTime ToStartOfHour(this DateTime dt) => dt.Millisecond == 0 && dt.Second == 0  && dt.Minute ==0? dt
        : new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind);

    public static TimeSpan SinceStartOfHour(this DateTime dt) => dt - dt.ToStartOfHour();
    public static DateTime ToStartOfDay(this DateTime dt) => dt.Hour == 0 && dt.Millisecond == 0 && dt.Second == 0 && dt.Minute == 0 ? dt
        : new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Kind);
    public static DateTime ToStartOfWeek(this DateTime dt)
    {
        int diff = dt.DayOfWeek - CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.FirstDayOfWeek;
        if (diff < 0) diff += 7;
        return dt.AddDays(-1 * diff).Date;
    }
    public static double TotalMicroSeconds(this TimeSpan ts) => ts.TotalMilliseconds * 1000;
    public static DateTime AddMicroSeconds(this DateTime dt, double us) => dt.AddMilliseconds(us/1000);
    public static bool IsStartOfHour(this DateTime dt) => (dt - dt.ToStartOfHour()).TotalSeconds == 0;
    public static bool IsWeekend(this DateTime dt) => dt.DayOfWeek == DayOfWeek.Sunday || dt.DayOfWeek == DayOfWeek.Saturday;
}