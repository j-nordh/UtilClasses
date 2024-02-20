using System;
using System.Collections.Generic;
using System.Text;
using UtilClasses.Extensions.Strings;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Enumerable = System.Linq.Enumerable;

namespace UtilClasses.Tests
{
    public class TreeSetTests
    {
        private readonly ITestOutputHelper _output;
        private TreeSet _treeSet;

        public TreeSetTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        void Setup()
        {
            _treeSet = new TreeSet();
            var arr = new[] {"a", "b", "c"};
            foreach (var l1 in arr)
            {
                _treeSet.Add(l1);
                foreach(var l2 in arr)
                {
                    _treeSet.Add($"{l1}.{l2}");
                    foreach(var l3 in arr) _treeSet.Add($"{l1}.{l2}.{l3}");
                }
            }
            Assert.Equal(39, _treeSet.Count);
            Assert.Equal(3, _treeSet.Depth);
        }
        [Fact]
        void GetAll()
        {
            Setup();
            var ret = _treeSet.All();
            Assert.Equal(39, ret.Count);
            Assert.Contains(ret, e => e.EqualsOic("a"));
            Assert.Contains(ret, e => e.EqualsOic("a.a"));
            Assert.Contains(ret, e => e.EqualsOic("b.c.c"));
        }
        [Theory]
        [InlineData(true, "a")]
        [InlineData(true, "b.b")]
        [InlineData(true, "c.b.a")]
        [InlineData(false, "f")]
        [InlineData(false, "a.f")]
        [InlineData(false, "a.a.f")]
        void Contains(bool match, string s)
        {
            Setup();
            try
            {
                Assert.Equal(match, _treeSet.Contains(s));
            }
            catch (XunitException)
            {
                _output.WriteLine(_treeSet.ToString());
                throw;
            }
            
        }
        [Theory]
        [InlineData(true, "a")]
        [InlineData(true, "b.b")]
        [InlineData(false, "c.b.a")]
        void HasNode(bool match, string s)
        {
            Setup();
            Assert.Equal(match, _treeSet.HasNode(s));
        }

        [Theory]
        [InlineData(1, "a")]
        [InlineData(2, "a", "b")]
        [InlineData(3, "a", "a.b", "b")]
        [InlineData(1, "a.b.c.d.e.f.g.h")]
        void Inserts(int count, params string[] strings )
        {
            var ts = new TreeSet();
            ts.AddRange(strings);
            Assert.Equal(count, ts.Count);
        }

        
        
    }
}
