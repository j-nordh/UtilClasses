using System;
using UtilClasses.Extensions.Booleans;

namespace UtilClasses
{
    public class Find
    {
        public static T First<T>(Func<T, bool> predicate, Func<int, T> convert, out int iterations, int start = 1)
        {
            Ensure.Argument.Is(start>0, nameof(start), "Starting point must be non-negative");
            iterations = 0;
            var val = convert(start);
            if (predicate(val)) return val;
            var floor = start;
            var ceiling = start;
            var last = 0;
            while (!predicate(convert(ceiling)))
            {
                floor = ceiling;
                ceiling *= 2;
            }
            
            while (true)
            {
                iterations += 1;
                var i = floor + (ceiling - floor) / 2;
                if (last == i) return convert(1 + i);
                last = i;
                if (predicate(convert(i))) ceiling = i;
                else floor = i;
            }
        }
        public static int FirstInt(Func<int, bool> f, int start = 1) => First(f, i=>i, out _, start);

        public static int FirstInt(Func<int, bool> f, out int iterations, int start = 1) =>
            First(f, i => i, out iterations, start);
    }
}
