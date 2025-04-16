using System;
using System.Collections.Generic;
using System.Text;
using UtilClasses.Core;
using Xunit;

namespace UtilClasses.Tests
{
    public class FindTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(17)]
        [InlineData(89)]
        [InlineData(46520)]
        public void Search(int val)
        {
            Assert.Equal(Find.FirstInt(i => i > val), val+1);
        }

        [Fact]
        public void ThrowsOnNeg() => Assert.Throws<ArgumentException>(() => Find.FirstInt(f => true, -1));
        [Theory]
        [InlineData(0,0)]
        [InlineData(1,2)]
        [InlineData(2,3)]
        [InlineData(10,5)]
        [InlineData(100,8)]
        [InlineData(1000,11)]
        [InlineData(46520,17)]
        public void Depth(int val, int expected)
        {
            Find.FirstInt(i => i > val, out int iterations);
            Assert.Equal(expected, iterations);
        }

        
    }
}
