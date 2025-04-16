using System;
using System.Text;
using UtilClasses.Core;

namespace ExtendedUtilClasses;

public class ReadableHashGenerator
{
    public static string Hash(string str) => Hash(Encoding.UTF8.GetBytes(str));
    public static string Hash(byte[] bytes)
    {
        var seed = BitConverter.ToInt32(System.IO.Hashing.Crc32.Hash(bytes));
        var gen = new IdGenerator.UnsafeGenerator(seed);
        return gen.Thing();
    }
}