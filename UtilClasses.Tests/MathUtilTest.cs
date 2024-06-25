using UtilClasses.MathClasses;
using Xunit;

namespace UtilClasses.Tests;

public class MathUtilTest
{
    [Theory]
    [InlineData(1,0,10,1)]
    [InlineData(-1,0,10,9)]
    [InlineData(11,0,10,1)]
    [InlineData(12,-10,10,-8)]
    [InlineData(-12,-10,10,8)]
    public void PositiveIntegers(int value, int min, int max, int expected)
    {
        Assert.Equal(expected, MathUtil.WrapAround(value, min, max));
    }
}