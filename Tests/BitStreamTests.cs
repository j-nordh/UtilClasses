using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Core;
using UtilClasses.Core.Extensions.BitArrays;
using UtilClasses.Core.Extensions.Bytes;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Enums;
using UtilClasses.Core.Extensions.Strings;
using Xunit;
using Xunit.Abstractions;
using static UtilClasses.Tests.BitStreamTestData;
namespace UtilClasses.Tests
{
    public class BitStreamTests
    {
        private readonly ITestOutputHelper _output;

        public BitStreamTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(GetFblEndian), MemberType = typeof(BitStreamTestData))]
        public void ConstructorEndian(FirstBitLocation fbl, bool bigEndian)
        {
            var stream = new BitStream(new byte[] { 1, 2, 3, 4 }, fbl, bigEndian);
            var bytes = stream.GetBytes();
            _output.WriteLine($"Actual bytes: {bytes[0]} {bytes[1]} {bytes[2]} {bytes[3]}");
            Assert.Equal(1, bytes[0]);
            Assert.Equal(2, bytes[1]);
            Assert.Equal(3, bytes[2]);
            Assert.Equal(4, bytes[3]);
        }
        [Theory]
        [MemberData(nameof(GetFblEndian), MemberType = typeof(BitStreamTestData))]
        public void WriteBytesEndian(FirstBitLocation fbl, bool bigEndian)
        {
            var stream = new BitStream(32, fbl, bigEndian);

            var vals = new byte[] { 1, 2, 3, 4 };
            _output.WriteLine(FormatBytes(vals));
            stream.WriteBytes(vals, 8);

            var bytes = stream.GetBytes();
            _output.WriteLine(FormatBytes(bytes));
            _output.WriteLine(BitConverter.ToString(bytes));
            var expected = new List<byte>();
            switch (fbl)
            {
                case FirstBitLocation.LastByteFirstBit:
                    bytes.AssertResult(0b00000100u, 0b00000011u, 0b00000010u, 0b00000001u);
                    break;
                case FirstBitLocation.LastByteLastBit:
                    bytes.AssertResult(0b00100000u, 0b11000000u, 0b01000000u, 0b10000000u);
                    break;
                case FirstBitLocation.FirstByteFirstBit:
                    bytes.AssertResult(0b00000001u, 0b00000010u, 0b00000011u, 0b00000100u);
                    break;
                case FirstBitLocation.FirstByteLastBit:
                    bytes.AssertResult(0b10000000u, 0b01000000u, 0b11000000u, 0b00100000u);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fbl), fbl, null);
            }
        }

        string AsInt(byte[] bs, bool bigEndian) => EndianBitConverter.ToInt32(bs, 0, bigEndian).ToString();

        [Theory]
        [MemberData(nameof(GetFblEndian), MemberType = typeof(BitStreamTestData))]
        public void WriteIntEndian(FirstBitLocation fbl, bool bigEndian)
        {
            var values = new[] { 1, 64, 1547861, -5 };
            _output.WriteLine("Input:");
            foreach (var i in values)
            {
                _output.WriteLine(FormatBytes(BitConverter.GetBytes(i), bs => AsInt(bs, bigEndian)));
            }
            var stream = new BitStream(128, fbl, bigEndian);
            values.ForEach(i => stream.Write(i));
            var bytes = stream.GetBytes();
            _output.WriteLine("");
            _output.WriteLine("Output:");
            foreach (var bs in bytes.Paginate(4))
            {
                _output.WriteLine(FormatBytes(bs, bs => BitConverter.ToInt32(bs).ToString()));
            }

            var expected = new Dictionary<FirstBitLocation, int[]>();

            void SetExpected(FirstBitLocation l, params int[] vals)
            {
                expected[l] = vals;
            }
            SetExpected(FirstBitLocation.LastByteFirstBit, -67108865, 1436423936, 1073741824, 16777216);
            SetExpected(FirstBitLocation.LastByteLastBit, -536870913, -1434851328, 33554432, -2147483648);
            SetExpected(FirstBitLocation.FirstByteFirstBit, 1, 64, 1547861, -5);
            SetExpected(FirstBitLocation.FirstByteLastBit, 128, 2, 15235498, -33);

            _output.WriteLine("");
            _output.WriteLine("Expected:");
            foreach (var i in expected[fbl])
                _output.WriteLine(FormatBytes(BitConverter.GetBytes(i), bs => AsInt(bs, bigEndian)));

            var ebc = new EndianBitConverter(bigEndian);
            bytes
                .Paginate(4)
                .AssertResult(expected[fbl].Select(ebc.GetBytes));
        }
        string FormatBytes(IEnumerable<byte> bytes, Func<byte[], string> extraFormatter = null)
        {
            var bs = bytes.ToArray();
            var dec = bs.Select(b => $"{b,3}").Join(" ");
            var hex = bs.Select(b => $"{b:X2}").Join("");
            var bin = bs.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Join(" ");
            var ps = extraFormatter == null ? "" : "\t" + extraFormatter(bs);
            return $"{dec}\t0x{hex}\t{bin}{ps}";
        }

        void DebugInt(int expected, int actual, bool bigEndian) => DebugBytes(BitConverter.GetBytes(expected), BitConverter.GetBytes(actual), bs => AsInt(bs, bigEndian));
        void DebugInt(int expected, int actual, byte[] stored, bool bigEndian) => DebugBytes(BitConverter.GetBytes(expected), BitConverter.GetBytes(actual), stored, bs => AsInt(bs, bigEndian));

        void DebugBytes(byte[] expected, byte[] actual, Func<byte[], string> extraFormatter = null) =>
            DebugBytes(expected, actual, null, extraFormatter);
        void DebugBytes(byte[] expected, byte[] actual, byte[] stored, Func<byte[], string> extraFormatter = null)
        {
            void printPaginated(byte[] bytes)
            {
                bytes.Paginate(4).ForEach(bs => _output.WriteLine(FormatBytes(bs, extraFormatter)));
            }
            _output.WriteLine("Expected:");
            printPaginated(expected);
            _output.WriteLine("Actual:");
            printPaginated(actual);
            if (stored == null) return;
            _output.WriteLine("Stored:");
            printPaginated(stored);
        }

        [Theory]
        [MemberData(nameof(GetFblEndian), MemberType = typeof(BitStreamTestData))]
        public void ReadbackIntEndian(FirstBitLocation fbl, bool bigEndian)
        {
            var stream = new BitStream(32, fbl, bigEndian);
            var rnd = new Random();

            var val = rnd.Next();
            stream.Write(val);
            var res = stream.ReadInt();
            DebugInt(val, res, stream.GetBytes(), bigEndian);
            Assert.Equal(val, res);
        }

        //[Theory]
        //[MemberData(nameof(GetFblEndianSize), MemberType = typeof(BitStreamTestData))]
        //public void ReadbackIntSize(FirstBitLocation fbl, bool bigEndian, int size)
        //{
        //    var stream = new BitStream(32, fbl, bigEndian);
        //    var rnd = new Random();

        //    var val = rnd.Next((int)Math.Pow(2, size) - 1);
        //    stream.Write(val, size);
        //    var res = stream.ReadInt(size);
        //    DebugInt(val, res, stream.GetBytes(), bigEndian);
        //    Assert.Equal(val, res);
        //}

        //[Theory]
        //[MemberData(nameof(GetSizeTestData))]
        //public void WriteIntSize(FirstBitLocation fbl, bool bigEndian, int size, int val)
        //{
        //    var stream = new BitStream(32, fbl, bigEndian);
        //    stream.Write(val, size);
        //    var bytes = stream.GetBytes();
        //    var bits = new BitArray(bytes);

        //    for (int i = 0; i < size; i++)
        //    {
        //        var org = (val & 1 << i) > 0;
        //        var index = stream.GetBufferIndex(i);
        //        Assert.True(org == bits[index], $"Check failed at bit {i}. Computed buffer index was {index}. Buffer: {bytes.ToHexString()}");
        //    }
        //}

        [Theory]
        [InlineData(1)]
        [InlineData(256)]
        [InlineData(65536)]
        [InlineData(16777216)]
        [InlineData(1073741824)]
        [InlineData(4194304)]
        [InlineData(16384)]
        [InlineData(64)]
        [InlineData(128)]
        [InlineData(32768)]
        [InlineData(8388608)]
        [InlineData(258)]
        [InlineData(66051)]
        public void GetLeastSignificantBits(int val)
        {
            foreach (var bigEndian in new[] { true, false })
            {
                var inBytes = EndianBitConverter.GetBytes(val, bigEndian);
                var inArr = new BitArray(inBytes);
                for (int i = 1; i < 32; i++)
                {
                    if (val >= Math.Pow(2, i)) continue;

                    var outBytes = BitArrayExtensions.LeastSignificantBits(val, i, bigEndian);

                    var outVal = EndianBitConverter.ToInt32(outBytes, bigEndian);
                    Assert.True(val == outVal, $@"In: {val} 
Out: {outVal} 
Bits: {i} 
BigEndian: {bigEndian} 
InBytes:  {inBytes.ToHexString()} 
OutBytes: {outBytes.ToHexString()}");
                }
            }

        }
        [Theory]
        [MemberData(nameof(GetFblEncStringsEndian) , MemberType = typeof(BitStreamTestData))]
        public void ReadBackFixedString(FirstBitLocation fbl, (Encoding enc, int bpc) e, string data, bool bigEndian)
        {
            var stream = new BitStream(8000, fbl, bigEndian);
            stream.Write(data, e.enc, e.bpc);
            var res = stream.ReadString(e.enc, e.bpc, data.Length);
            Assert.Equal(data, res);
        }
        //[Fact]
        //public void PrintFblEndian()
        //{
        //    var res = BitStreamTestData.GetFblEndian(DataType.Encoding | DataType.Strings).ToList();
        //    _output.WriteLine(res.Select(r=>$"{r.Length}").Join("\n"));
        //}

        [Theory]
        [MemberData(nameof(GetFblEncStringsEndian), MemberType = typeof(BitStreamTestData))]
        public void ReadBackStringUntilNull(FirstBitLocation fbl, (Encoding enc, int bpc) e, string data, bool bigEndian)
        {
            var stream = new BitStream(8000, fbl, bigEndian);
            stream.Write(data, e.enc, e.bpc);
            var res = stream.ReadString(e.enc, e.bpc);
            Assert.Equal(data, res);
        }


        //Magic: http://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith64Bits
        private byte Reverse(byte b) => BitConverter.GetBytes(((b * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32).First();


        public static IEnumerable<object[]> GetSizeTestData()
        {
            var values = new[] { 1, 256, 257 };
            foreach (var fbl in Enum<FirstBitLocation>.Values)
                foreach (var endian in new[] { true, false })
                    foreach (var size in Enumerable.Range(1, 31))
                        foreach (var val in values)
                        {
                            if (val > Math.Pow(2, size)) continue;
                            yield return new object[] { fbl, endian, size, val };
                        }
        }

        //public static IEnumerable<object[]> GetFblEndian() =>
        //    Enum<FirstBitLocation>.Values.CombinatoryZip(new[] { true, false },
        //        (fbl, endian) => new object[] { fbl, endian });


    }


    static class BitStreamTestExtensions
    {
        public static void Add(this List<byte> lst, byte val) => lst.Add(val);
        public static void AddRange(this List<byte> lst, IEnumerable<byte> val) => val.ForEach(lst.Add);

        public static void AssertResult(this byte[] actual, params uint[] expected) =>
            actual.AssertResult(expected.Select(v => (byte)v).ToArray());
        public static void AssertResult(this byte[] actual, params byte[] expected)
        {
            actual.Select(b => $"{b:X}").Join("-");
            for (int i = 0; i < actual.Length; i++)
                Assert.Equal((byte)expected[i], actual[i]);
        }
        public static void AssertResult(this IEnumerable<IEnumerable<byte>> bytes, params byte[][] expected)
        {
            var lst = bytes.Select(x => x.ToArray()).ToList();
            for (int i = 0; i < lst.Count; i++)
                AssertResult(lst[i], expected[i]);
        }

        public static void AssertResult(this IEnumerable<IEnumerable<byte>> bytes,
            IEnumerable<IEnumerable<byte>> expected) => bytes.AssertResult(expected.Select(e => e.ToArray()).ToArray());
        public static void AssertResult(this IEnumerable<IEnumerable<byte>> bytes, params int[] expected)
        {
            var lst = bytes.Select(x => x.ToArray()).ToList();
            for (int i = 0; i < lst.Count; i++)
                AssertResult(lst[i], BitConverter.GetBytes(expected[i]));
        }
    }
}
