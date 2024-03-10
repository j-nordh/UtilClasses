using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Interfaces;
using UtilClasses.Extensions.DateTimes;

namespace UtilClasses
{
    public class TimeZoneFixer
    {
        private readonly TimeZoneInfo _default;

        public TimeZoneFixer(string id)
        {
            _default = TimeZoneInfo.FindSystemTimeZoneById(id);
        }

        public static Action<IHasTimestamp> GetFixer(TimeZoneInfo tzi) => o => Fix(o, tzi);
        public static Action<IHasTimestamp> GetFixer(string timeZoneId) => o => Fix(o, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        public static Action<T> GetFixer<T>(string timeZoneId) where T : IHasTimestamp => o => Fix(o, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

        public Action<IHasTimestamp> GetFixer() => o => Fix(o, _default);

        public void FixDefault(IHasTimestamp o) => Fix(o, _default);

        public static void Fix(IHasTimestamp x, TimeZoneInfo tzi)
        {
            x.Timestamp = Fix(x.Timestamp, tzi);
        }

        public bool IsAmbiguous(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc) return false;
            return _default.IsAmbiguousTime(dt);
        }
        public bool IsInvalid(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc) return false;
            return _default.IsInvalidTime(dt);
        }

        public DateTime Fix(DateTime dt) => Fix(dt, _default);
        public static DateTime Fix(DateTime dt, TimeZoneInfo tzi)
        {
            if (dt.Kind == DateTimeKind.Utc) return dt;
            if (TimeZoneInfo.Utc.Equals(tzi)) return new DateTime(dt.Ticks, DateTimeKind.Utc);
            if (dt.Kind == DateTimeKind.Local) dt = dt.WithKind(DateTimeKind.Unspecified);
            if (tzi.IsAmbiguousTime(dt)) throw new ArgumentException("The specified datetime is ambiguous");
            try
            {
                return TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public static IEnumerable<T> Fix<T>(IEnumerable<T> items, TimeZoneInfo tzi) where T : IHasTimestamp
        {
            bool foundAmbi = false;
            var lst = items.ToList();
            foreach(var i in lst)
            {
                i.Timestamp = Fix(i.Timestamp, tzi, ref foundAmbi);
            }
            return lst;
        }
        public static IEnumerable<DateTime> Fix(IEnumerable<DateTime> items, TimeZoneInfo tzi)
        {
            bool foundAmbi = false;
            return items.Select(i => Fix(i, tzi, ref foundAmbi));
        }
        public static DateTime Fix(DateTime dt, TimeZoneInfo tzi, ref bool foundAmbi)
        {
            if (tzi.IsAmbiguousTime(dt))
            {
                dt = foundAmbi
                    ? Fix(dt.AddHours(1), tzi).AddHours(-1)
                    : Fix(dt.AddHours(-1), tzi).AddHours(1);
                foundAmbi = !foundAmbi;
            }
            else if (tzi.IsInvalidTime(dt))
                dt = Fix(dt.AddHours(1), tzi);
            else
                dt = Fix(dt, tzi);
            return dt;
        }
        public IEnumerable<T> Fix<T>(IEnumerable<T> items) where T : IHasTimestamp => Fix(items, _default);
        public IEnumerable<DateTime> Fix(IEnumerable<DateTime> items) => Fix(items, _default);

        public DateTime Reverse(DateTime dt) => Reverse(dt, _default);
        public static DateTime Reverse(DateTime dt, TimeZoneInfo tzi)
        {
            if (dt.Kind == DateTimeKind.Local) dt = dt.WithKind(DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeFromUtc(dt, tzi);
        }
    }
    public static class TimeZoneFixerExtensions
    {
        public static IEnumerable<T> FixTimestamps<T>(this IEnumerable<T> items, TimeZoneFixer tzf) where T : IHasTimestamp
         => tzf.Fix(items);
        public static IEnumerable<T> FixTimestamps<T>(this IEnumerable<T> items, string timeZoneId) where T : IHasTimestamp
         => new TimeZoneFixer(timeZoneId).Fix(items);
    }
}
