using System;
using UtilClasses.Core.Extensions.Doubles;

namespace UtilClasses.Core.Extensions.Numbers;

public static  class NumberExtensions
{
    public static double Squared(this double d) => d * d;
    public static double ToRad(this double d) => d*Math.PI/180;
    public static float ToRadF(this double degrees) => degrees.AsFloat() * PIF / 180.0f;
    public static float ToRad(this float d) => d*PIF/180;
    public static float ToDegrees(this float rad) => rad * 180 / PIF;
    public static float ToDegreesF(this double rad) => ((float)rad) * 180 / PIF;

    private static readonly float PIF = (float)Math.PI;
}