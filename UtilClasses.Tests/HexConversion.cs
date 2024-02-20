using UtilClasses.Extensions.Bytes;
using UtilClasses.Extensions.Strings;
using Xunit;

namespace UtilClasses.Tests
{
    public class HexConversion
    {
        [Theory]
        [InlineData("01", new byte[] { 1 })]
        [InlineData("0102", new byte[] { 1, 2 })]
        [InlineData("010203", new byte[] { 1, 2, 3 })]
        [InlineData("01020304", new byte[] { 1, 2, 3, 4 })]
        public void FromHex(string hex, byte[] expected)

        {
            var ret = hex.HexToByteArray();
            Assert.Equal(expected.Length, ret.Length);
            for (int i = 0; i < ret.Length; i++)
                Assert.Equal(expected[i], ret[i]);
        }

        [Theory]
        [InlineData(new byte[] { 1 }, "01")]
        [InlineData(new byte[] { 1, 2 }, "0102")]
        [InlineData(new byte[] { 1, 2, 3 }, "010203")]
        [InlineData(new byte[] { 1, 2, 3, 4 }, "01020304")]
        public void ToHex(byte[] bytes, string expected)
        {
            var ret = bytes.ToHexString();
            Assert.Equal(expected.ToUpper(), ret.ToUpper());
        }
    }
}

