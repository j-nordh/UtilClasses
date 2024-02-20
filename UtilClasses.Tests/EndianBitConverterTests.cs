using System.Xml;
using UtilClasses.Extensions.Strings;
using Xunit;

namespace UtilClasses.Tests
{
    public class EndianBitConverterTests
    {
        [Theory]
        [InlineData(1, true, "00000001")]
        [InlineData(256, true, "00000100")]
        [InlineData(65536, true, "00010000")]
        [InlineData(16777216, true, "01000000")]
        [InlineData(16777216, false, "00000001")]
        [InlineData(65536, false, "00000100")]
        [InlineData(256, false, "00010000")]
        [InlineData(1, false, "01000000")]
        void IntToByte(int i, bool big, string hex)
        {
            AssertEqual(EndianBitConverter.GetBytes(i, big), hex.HexToByteArray());
        }

        void AssertEqual(byte[] actual, params byte[] expected)
        {
            for (var i = 0; i < actual.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }
        [Theory]
        [InlineData(1, true, "00000001")]
        [InlineData(256, true, "00000100")]
        [InlineData(65536, true, "00010000")]
        [InlineData(16777216, true, "01000000")]
        [InlineData(16777216, false, "00000001")]
        [InlineData(65536, false, "00000100")]
        [InlineData(256, false, "00010000")]
        [InlineData(1, false, "01000000")]
        void ByteToInt(int expected, bool big, string hex)
        {
            Assert.Equal(expected, EndianBitConverter.ToInt32(hex.HexToByteArray(), big));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(3542457, true)]
        [InlineData(3542457, false)]
        void CopyBytes(int i, bool big)
        {
            var bs = new byte[4];
            EndianBitConverter.CopyBytes(i, bs, 0, big);
            Assert.Equal(i, EndianBitConverter.ToInt32(bs, big));

            bs = new byte[6];
            EndianBitConverter.CopyBytes(i, bs, 1, big);
            Assert.Equal(0, bs[0]);
            Assert.Equal(0, bs[5]);
            Assert.Equal(i, EndianBitConverter.ToInt32(bs, 1, big));
        }
    }
}
