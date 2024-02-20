using System;
using System.Collections;

namespace UtilClasses.Extensions.BitArrays
{
    public static class BitArrayExtensions
    {
        public static byte[] ToByteArray(this BitArray bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            numBytes += 1; //this ensures that the resulting integer is unsigned.

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << bitIndex);

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        public static byte[] LeastSignificantBits(int val, int bits, bool bigEndian)
        {
            var inBytes = EndianBitConverter.GetBytes(val, bigEndian);
            var arr = new BitArray(inBytes);
            var byteCount = (int)Math.Ceiling(arr.Length / 8.0);
            var retSize = (int)Math.Ceiling(bits / 8.0);

            var ret = new BitArray(retSize * 8);

            if (bigEndian)
            {
                var offset = (byteCount - retSize) * 8;
                for (int i = 0; i < bits; i++)
                {
                    var index = (retSize - 1 - i / 8) * 8 + i % 8;
                    ret[index] = arr[index + offset];
                }
            }
            else
            {
                for (int i = 0; i < bits; i++)
                {
                    ret[i] = arr[i];
                }
            }
            var bytes = new byte[retSize];
            ret.CopyTo(bytes, 0);
            return bytes;
        }

    }
}
