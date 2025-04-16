using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Core;
using UtilClasses.Core.Extensions.Enums;

namespace UtilClasses.Tests;

class BitStreamTestData
{
    [Flags]
    public enum DataType
    {
        FirstBitLocation = 1 << 0,
        Endian = 1 << 1,
        Size = 1 << 2,
        Strings = 1 << 3,
        IntSingles = 1 << 4,
        Encoding = 1 << 5,
        All = ~0
    }
    private static readonly string[] STRINGS = {
        "In a hole in the ground there lived a hobit.",
        ",./_@",
        "The quick brown fox jumps over the lazy dog",
        "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore",
    };

    private static readonly int[] SINGLE_BITS = {
        1,
        2,
        4,
        8,
        16,
        32,
        64,
        128,
        256,
        512,
        1024,
        2048,
        4096,
        8192,
        16384,
        32768,
        65536,
        131072,
        262144,
        524288,
        1048576,
        2097152,
        4194304,
        8388608,
        16777216,
        33554432,
        67108864,
        134217728,
        268435456,
        536870912,
        1073741824,
        -2147483648
    };

    private static readonly (Encoding enc, int bpc)[] ENCODING_TYPES =
    {
        (Encoding.ASCII, 8),
        (Encoding.ASCII, 7),
        (Encoding.UTF8, 8),
        (Encoding.Unicode, 16),
    };

    public static IEnumerable<object[]> GetFblEndian() =>
        GetOrdered(DataType.FirstBitLocation, DataType.Endian);
    public static IEnumerable<object[]> GetOrdered(params DataType[] sources)
    {
        var src = new List<List<object>>();

        foreach (var s in sources)
        {
            var data = s switch
            {
                DataType.FirstBitLocation => Enum<FirstBitLocation>.Values.Cast<object>(),
                DataType.Endian => new[] { true, false }.Cast<object>(),
                DataType.Size => Enumerable.Range(1, 32).Cast<object>(),
                DataType.Strings => STRINGS,
                DataType.IntSingles => SINGLE_BITS.Cast<object>(),
                DataType.Encoding => ENCODING_TYPES.Cast<object>(),
                _ => throw new ArgumentOutOfRangeException()
            };
            src.Add(data.ToList());
        }

        return Zip(src.ToArray());
    }
    public static IEnumerable<object[]> GetFblEndianSize() =>
        Zip(Enum<FirstBitLocation>.Values.Cast<object>().ToList(),
            new[] { true, false }.Cast<object>().ToList(),
            Enumerable.Range(1, 31).Cast<object>().ToList());

    //FirstBitLocation fbl, (Encoding enc, int bpc) e, string data, bool bigEndian

    public static IEnumerable<object[]> GetFblEncStringsEndian()
    {
        var ret = GetOrdered(DataType.FirstBitLocation, DataType.Encoding, DataType.Strings, DataType.Endian).ToList();
        return ret;
    }

    private static List<object[]> Zip(params List<object>[] os)
    {
        if (!os.Any())
            throw new ArgumentOutOfRangeException();
        if (os.Count() == 1)
            return os.First().Select(o=>new[]{o}).ToList();

        var ret = new List<object[]>();
        var myVals = os.First().ToList();
        foreach (var val in myVals)
        {
            foreach (var res in Zip(os.Skip(1).ToArray()))
            {
                var lst = new List<object>() { val };
                lst.AddRange(res);
                ret.Add(lst.ToArray());
            }
        }

        return ret;
    }
}