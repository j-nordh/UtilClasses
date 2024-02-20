using System;
using System.Globalization;

namespace UtilClasses.Extensions.Doubles
{
    public static class DoubleExtensions
    {
        public static double Abs(this double d) => Math.Abs(d);
        public static decimal AsDecimal(this double d) => (decimal)d;
        public static decimal AsDecimal(this float d) => (decimal)d;
        public static bool Near(this double d, double o, double limit = 0.0001) => Math.Abs(d - o) < limit;
        public static bool Near(this float d, float o, float limit = 0.0001f) => Math.Abs(d - o) < limit;
        public static double AsDouble(this object o) => o is double d ? d : o.ToString().AsDouble();
        public static double AsDouble(this string s) => double.Parse(s.Replace('.', ','), NumberStyles.Any, CultureInfo.GetCultureInfo("SV-se"));
        public static bool IsDouble(this string s) => s.IsDouble(out _);
        public static bool IsDouble(this string s, out double res) => double.TryParse(s.Replace('.', ','), NumberStyles.Any, CultureInfo.GetCultureInfo("SV-se"), out res);
        public static double? MaybeAsDouble(this string s) => s.IsDouble(out var res) ? res : null;

        public static double? MaybeAsDouble(this object o) =>
            o switch
            {
                null => null,
                double d => d,
                float f => f,
                int i => i,
                _ => o.ToString().MaybeAsDouble()
            };
            
        public static float AsFloat(this object o) => o switch
        {
            float f => f,
            double d => (float)d,
            int i => i,
            _ => o.ToString().AsFloat()
        };

        public static float? MaybeAsFloat(this string s) => s.IsFloat(out var ret) ? ret : null;  

        public static float AsFloat(this string s) =>
            float.Parse(s.Replace(".", ","), NumberStyles.Any, CultureInfo.GetCultureInfo("SV-se"));
        public static bool IsFloat(this string s, out float res) => float.TryParse(s.Replace('.', ','), NumberStyles.Any, CultureInfo.GetCultureInfo("SV-se"), out res);
        public static bool IsFloat(this string s) => s.IsFloat(out var _);
    }
}
