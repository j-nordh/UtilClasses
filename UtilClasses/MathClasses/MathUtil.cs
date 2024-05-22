using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace UtilClasses.MathClasses
{
    public static class MathUtil
    {
        public static decimal Map(decimal val, decimal inLow, decimal inHigh, decimal toLow, decimal toHigh, bool cap =false)
        {
            var ret = (val - inLow) * (toHigh - toLow) / (inHigh - inLow) + toLow;
            if (cap && ret < toLow) return toLow;
            if (cap && ret > toHigh) return toHigh;
            return ret;
        }
        public static float Map(float val, float inLow, float inHigh, float toLow, float toHigh, bool cap = false)
        {
            var ret = (val - inLow) * (toHigh - toLow) / (inHigh - inLow) + toLow;
            if (cap && ret < toLow) return toLow;
            if (cap && ret > toHigh) return toHigh;
            return ret;
        }
        public static double Map(double val, double inLow, double inHigh, double toLow, double toHigh, bool cap = false)
        {
            var ret = (val - inLow) * (toHigh - toLow) / (inHigh - inLow) + toLow;
            if (cap && ret < toLow) return toLow;
            if (cap && ret > toHigh) return toHigh;
            return ret;
        }
        public static void CalculateLinearCoefficients(double inputMin, double inputMax, double outputMin, double outputMax, out double slope, out double yIntercept)
        {
            slope = (outputMax - outputMin) / (inputMax - inputMin);
            yIntercept = outputMin - (slope * inputMin);
        }

        public static decimal Interpolate(decimal x0, decimal y0, decimal x1, decimal y1, decimal x)
            =>    y0 * (x1 - x) + y1*(x - x0) / (x1 - x0);
        public static float Interpolate(float x0, float y0, float x1, float y1, float x)
            => y0 * (x1 - x) + y1 * (x - x0) / (x1 - x0);

        public static float Interpolate(PointF p0, PointF p1, float x) => Interpolate(p0.X, p0.Y, p1.X, p0.Y, x);

        public static int WrapAround(int value, int min, int max)
        {
            if (value >= min && value <= max)
                return value;
            var r = max - min;
            while (value < min)
            {
                value += r;
            }
            while (value > max)
            {
                value -= r;
            }

            return value;
        }

    }
}
