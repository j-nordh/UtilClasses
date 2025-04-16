using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Enums;
using UtilClasses.Extensions.Strings;
using Color = System.Drawing.Color;

namespace UtilClasses.Winforms.Extensions
{
    public static class ColorExtensions
    {
        public static Color Brighten(this Color c, int amount)
        {
            return Color.FromArgb(
                c.A,
                Math.Min(255, c.R + amount),
                Math.Min(255, c.G + amount),
                Math.Min(255, c.B + amount));
        }

        public static Color AsColor(this string s)
        {
            var ret = s.MaybeAsColor();
            if (ret != null) return ret.Value;
            throw new ArgumentException("The supplied argument is not a properly formatted hex string");
        }
        public static Color? MaybeAsColor(this string s)
        {
            if (s.IsNullOrEmpty()) return null;
            s = s.Trim(' ', '#');
            var kc = Enum<KnownColor>.TryParse(s);
            if (kc != null) return Color.FromKnownColor(kc.Value);


            var bs = s.HexToByteArray();
            switch (bs.Length)
            {
                case 3: return Color.FromArgb(bs[0], bs[1], bs[2]);
                case 4: return Color.FromArgb(bs[0], bs[1], bs[2], bs[3]);
                default: return null;
            }
        }
    }
}
