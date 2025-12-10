namespace UtilClasses.Extensions.Numbers;

using System;
using System.Globalization;
using System;
using System.Globalization;

public static class SignficantFiguresExtensions
{
    /* -------------------------------------------------------------------- */
    /*  Numeric overloads – double, decimal, float                            */
    /* -------------------------------------------------------------------- */

    public static double RoundToSF(this double value, int sf)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return value;
        if (value == 0.0   || sf <= 0)       return value;

        double abs = Math.Abs(value);
        int    exp = (int)Math.Floor(Math.Log10(abs));

        double factor  = Math.Pow(10, exp - (sf - 1));
        double scaled   = value / factor;
        double rounded  = Math.Round(scaled, MidpointRounding.AwayFromZero);
        return rounded * factor;
    }

    public static decimal RoundToSF(this decimal value, int sf)
    {
        if (value == 0m || sf <= 0) return value;

        double dbl      = (double)value;
        int    exp      = (int)Math.Floor(Math.Log10(Math.Abs(dbl)));

        double factor   = Math.Pow(10, exp - (sf - 1));
        decimal scaled   = value / (decimal)factor;
        decimal rounded  = Math.Round(scaled, MidpointRounding.AwayFromZero);
        return rounded * (decimal)factor;
    }

    // NEW: float overload
    public static float RoundToSF(this float value, int sf)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) return value;
        if (value == 0f || sf <= 0)          return value;

        // Promote to double, round, then cast back
        double rounded = ((double)value).RoundToSF(sf);
        return (float)rounded;
    }

    /* -------------------------------------------------------------------- */
    /*  String overload – with optional “scientific” flag                    */
    /* -------------------------------------------------------------------- */

    /// <summary>
    /// Rounds the numeric value represented by *text* to *sf* significant
    /// figures and returns a string.  
    /// <paramref name="useScientific"/> (default = true) controls whether the
    /// result is forced into scientific notation when it would shorten the
    /// representation.
    /// </summary>
    public static string RoundToSF(this string text, int sf,
                                   bool useScientific = true)
    {
        if (!double.TryParse(text,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out double d))
        {
            throw new ArgumentException("The supplied text is not a valid number.", nameof(text));
        }

        // 1. Round numerically
        double rounded = d.RoundToSF(sf);

        if (rounded == 0.0)
            return "0";

        // 2. Decide format
        if (useScientific &&
           (Math.Abs(rounded) < 1e-3 || Math.Abs(rounded) >= Math.Pow(10, sf)))
        {
            // Exponential notation – (sf‑1) digits after the point
            return rounded.ToString($"E{sf - 1}", CultureInfo.InvariantCulture);
        }
        else
        {
            // Plain decimal notation
            int decPlaces = CalculateDecimalPlaces(rounded, sf);
            string fmt = $"F{decPlaces}";
            return rounded.ToString(fmt, CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Helper that calculates how many decimal places are required so that
    /// the resulting string contains exactly *sf* significant digits.
    /// </summary>
    private static int CalculateDecimalPlaces(double val, int sf)
    {
        double abs = Math.Abs(val);
        if (abs == 0.0) return 0;

        int integerDigits = (int)Math.Floor(Math.Log10(abs)) + 1;
        int decPlaces = Math.Max(0, sf - integerDigits);
        return decPlaces;
    }

    /* -------------------------------------------------------------------- */
    /*  Convenience overload – returns the rounded value as a double        */
    /* -------------------------------------------------------------------- */

    public static double ToRoundedDouble(this string text, int sf) =>
        double.Parse(text.RoundToSF(sf), CultureInfo.InvariantCulture);

    /* -------------------------------------------------------------------- */
    /*  Utility – convert string to double (used by ToRoundedDouble)         */
    /* -------------------------------------------------------------------- */

    private static double Parse(string s)
        => double.Parse(s, CultureInfo.InvariantCulture);
    
    /* -------------------------------------------------------------------- */
    /*  NEW:  *string* returning overload for float                          */
    /* -------------------------------------------------------------------- */

    public static string RoundToSFString(this float value, int sf,
        bool useScientific = true)
    {
        return value.ToString(CultureInfo.InvariantCulture)
            .RoundToSF(sf, useScientific);
    }

    public static string RoundToSFString(this double value, int sf,
        bool useScientific = true)
    {
        return value.ToString(CultureInfo.InvariantCulture)
            .RoundToSF(sf, useScientific);
    }

    public static string RoundToSFString(this decimal value, int sf,
        bool useScientific = true)
    {
        return value.ToString(CultureInfo.InvariantCulture)
            .RoundToSF(sf, useScientific);
    }
}
