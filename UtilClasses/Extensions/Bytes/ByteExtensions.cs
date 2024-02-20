using System;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Strings;


namespace UtilClasses.Extensions.Bytes
{
    public static class ByteExtensions
    {
        public static Encoding GetEncoding(this byte[] bs, out bool hasBom)
        {
            hasBom = true;
            if(bs.StartsWith(0xEF, 0xBB,0xBF)) return Encoding.UTF8;
            if(bs.StartsWith(0xFE, 0xFF))return Encoding.BigEndianUnicode;
            if(bs.StartsWith(0xFF,0xFE)) return Encoding.Unicode;
            if(bs.StartsWith(0,0,0xfe,0xff)) return Encoding.BigEndianUnicode;
            if(bs.StartsWith(0xff,0xfe,0,0)) return Encoding.Unicode;
            hasBom = false;
            return Encoding.UTF8;
        }

        public static Encoding GetEncoding(this byte[] bs)
        {
            bool hasBom;
            return bs.GetEncoding(out hasBom);
        }

        public static bool StartsWith(this byte[] bs, params byte[] start)
        {
            if (bs.Length < start.Length) return false;
            for (int i = 0; i < start.Length; i += 1)
            {
                if (bs[i] != start[i]) return false;
            }
            return true;
        }

        public static string ToHexString(this byte[] bs, string separator ="") => bs.Select(b=> $"{b:x2}").Join(separator);
        public static string ToBase64String(this byte[] bs) => Convert.ToBase64String(bs);

        public static int IndexOf(this byte[] bs, byte[] needle)
        {
            for (int i = 0; i < bs.Length; i++)
            {
                if (i + needle.Length > bs.Length) return -1;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (bs[i + j] != needle[j]) break;
                    if(j<needle.Length-1) continue;
                    return i;
                }
            }

            return -1;
        }

        public static byte GetCheckByte(this byte[] bs) => GetCheckByte(bs, 0, bs.Length);
        public static byte GetCheckByte(this byte[] bs, int offset, int count)
        {
            byte check = 0;
            for (int i = offset; i < count; i++)
            {
                check ^= bs[i];
            }

            return check;
        }

    }
}
