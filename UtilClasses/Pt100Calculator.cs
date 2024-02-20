using System;

namespace UtilClasses;
public class Pt100Calculator
{
    private static readonly double[] Di = new[]
    {
        439.932854, 472.41802, 37.684494,
        7.472018, 2.920828, 0.005184,
        -0.963864, -0.188732, 0.191203,
        0.049025,
    };

    private static readonly double[] Bi = new[]
    {
        0.183324722, 0.240975303, 0.209108771,
        0.190439972, 0.142648498, 0.077993465,
        0.012475611, -0.032267127, -0.075291522,
        -0.05647067, 0.076201285, 0.123893204,
        -0.029201193, -0.091173542, 0.001317696,
        0.026025526f
    };

    private readonly double _r001;
    private readonly double _b;
    private readonly double _a;

    public Pt100Calculator(double r001, double a, double b)
    {
        _r001 = r001;
        _a = a;
        _b = b;
    }

    public float GetTemp(float r) => (float) GetTemp((double) r);

    public double GetTemp(double r)
    {
        var w = r / _r001;
        var wr = w - _a * (w - 1) - _b * Math.Pow(w - 1, 2);
        double t;
        if (r >= _r001)
        {
            t = Di[0];
            var @base = (wr - 2.64) / 1.64;
            for (int i = 1; i < Di.Length; i++)
                t += Di[i] * Math.Pow(@base, i);
        }
        else
        {
            t = Bi[0];
            var @base = (Math.Pow(wr, 1.0 / 6) - 0.65) / 0.35;
            for (int i = 1; i < Bi.Length; i++)
                t += Bi[i] * Math.Pow(@base, i);

            t = t * 273.16 - 273.15;
        }
        return t is >= -60 and <= 156
            ? t
            : -300d;  //out of bounds for the calculation parameters, let's return something silly. That won't crash any rockets or so...

    }
}