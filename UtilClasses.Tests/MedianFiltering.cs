using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.MathClasses;
using Xunit;

namespace UtilClasses.Tests
{
    public class MedianFiltering
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public void ShrinkLength(List<decimal> test, int windowSize)
        {
            //Assert.Equal(5, test.Count);
            Assert.Equal(test.Count(), test.MedianFiltering(Filtering.Boundary.WindowShrink, EnumerableExtensions.Median, windowSize).Count());
        }

        [Theory]
        [MemberData(nameof(KnowData))]
        public void ShrinkFiltering(List<decimal> test, int windowSize)
        {
            //Assert.Equal(5, test.Count);
            var data = (windowSize == 3) ? new List<decimal>(){ 3,4,9,4,8,6,6,2,2,9 }
                : new List<decimal>(){ 3, 4, 4, 8, 6, 6, 3, 6, 6, 9 };
            var filteredData = test.MedianFiltering(Filtering.Boundary.WindowShrink, EnumerableExtensions.Median, windowSize);
            Assert.Equal(data, filteredData);
        }

        [Theory]
        [MemberData(nameof(KnowData))]
        public void ShrinkInequality(List<decimal> test, int windowSize)
        {
            //Assert.Equal(5, test.Count);
            var data = new List<decimal>() { 3, 4, 9, 4, 8, 6, 6, 2, 2, 8 };
            var filteredData = test.MedianFiltering(Filtering.Boundary.WindowShrink, EnumerableExtensions.Median, windowSize);
            Assert.NotEqual(data, filteredData);
        }

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] { new List<decimal>() { 1, 2, 3, 4, 5 }, 3};
            yield return new object[] { new List<decimal>() { 1, 2, 3, 4, 5 }, 5};
        }

        public static IEnumerable<object[]> KnowData()
        {
            yield return new object[] { new List<decimal>() { 3, 9, 4, 52, 3, 8, 6, 2, 2, 9}, 3};
            yield return new object[] { new List<decimal>() { 3, 9, 4, 52, 3, 8, 6, 2, 2, 9 }, 5};
        }
    }
}
