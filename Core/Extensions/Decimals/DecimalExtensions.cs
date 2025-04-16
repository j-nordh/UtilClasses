using System;
using System.Globalization;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Extensions.Decimals;

public static class DecimalExtensions
{
    public static string ToSaneString(this decimal d, int? decimalPlaces=null)
    {
        var nfi = new NumberFormatInfo() {NumberDecimalSeparator = ".", NumberGroupSeparator = ""};
        if (decimalPlaces.HasValue)
        {
            nfi.NumberDecimalDigits = decimalPlaces.Value;
            d = Math.Round(d, decimalPlaces.Value);
        }
        return d.ToString(nfi);
    }

    public static int AsInt(this decimal d) => (int)Math.Round(d, 0);

    public static decimal AsDecimal(this string s) => decimal.Parse(s.Replace(",", ".").RemoveAllWhitespace(), NumberStyles.Any, CultureInfo.InvariantCulture);
    public static decimal AsDecimal(this object obj) => obj as decimal? ?? obj.ToString().AsDecimal();

    public static bool IsDecimal(this string s) => !s.IsNullOrWhitespace() && decimal.TryParse(s.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    public static decimal? MaybeAsDecimal(this string s) => s.IsNullOrWhitespace()
        ? null
        : decimal.TryParse(s.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal ret)
            ? ret
            : null;

    public static decimal? MaybeAsDecimal(this object o) => o?.ToString().MaybeAsDecimal();
    public static bool Equals(this decimal? dec, decimal? other, int decimalPlaces)
    {
        if (null == dec) return other == null;
        if (null == other) return false;
        return Equals(dec.Value, other.Value, decimalPlaces);
    }
    public static bool Equals(this decimal dec, decimal other, int decimalPlaces) => Math.Round(dec, decimalPlaces) == Math.Round(other, decimalPlaces);

    public static decimal? Round(this decimal? val, int places) =>
        val.HasValue ? (decimal?)Math.Round(val.Value, places) : null;
}