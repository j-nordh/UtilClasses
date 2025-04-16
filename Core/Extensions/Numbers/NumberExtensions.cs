using System;

namespace UtilClasses.Core.Extensions.Numbers;

public static  class NumberExtensions
{
    public static double Squared(this double d) => d * d;
    public static double ToRad(this double d) => d*Math.PI/180;
    public static double FromRad(this double d) => d*180/Math.PI;
}