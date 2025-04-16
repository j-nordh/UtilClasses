using System;

namespace UtilClasses.Core.Extensions.MathExtensions;

public static class MathStuff
{
    public static int FloorInt(this double d) => (int) Math.Floor(d);
    public static int CeilingInt(this double d) => (int)Math.Ceiling(d);
    public static int RoundInt(this double d) => (int)Math.Round(d,0);
    public static int RoundInt(this float d) => (int)Math.Round(d, 0);
    public static int RoundInt(this decimal d) => (int)Math.Round(d, 0);
    public static ulong RoundUlong(this double d)=> (ulong)Math.Round(d, 0);
}