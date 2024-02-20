using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Strings;
using Xunit;

namespace UtilClasses.Tests
{
    public class LineBufferTests
    {
        private Dictionary<int, string> _texts = new()
        {
            [1] =
                "There once was a man from Nantucket\nWho kept all his cash in a bucket.\nBut his daughter, named Nan,\nRan away with a man\nAnd as for the bucket, Nantucket.",
            [2] = "En känd alpinist ifrån Bayern\r\nvar alltför förtjust i tokayern.\r\n    Han en linbana tog\r\n\tmen störta och dog\r\nty hans andedräkt frätte på wiren.  ",
            [3] = "\n\n\n\n",
            [4] = "\r\n\r\n\r\n\r\n",
            [5] = @"



 "
        };

        private List<int> _steps = new()
        {
            1, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 
            31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 
            73, 79, 83, 89, 97, 101, 103, 107, 109, 
            113, 127, 131, 137, 139, 149, 151, 157, 
            163, 167, 173, 179, 181, 191, 193, 197, 
            199, 211, 223, 227, 229, 233, 239, 241, 
            251, 257, 263, 269, 271, 277, 281, 283, 
            293, 307, 311, 313, 317, 331, 337, 347, 
            349, 353, 359, 367, 373, 379, 383, 389, 
            397, 401, 409
        };

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void Reconstruct(int i)
        {
            foreach (var s in _steps)
            {
                var txt = _texts[i];
                var lb = new LineBuffer(true);
                lb.Bites(_texts[i], s);
                if (txt.SplitLines().Last().IsNullOrEmpty())
                {
                    txt = txt.Substring(0, txt.Length - (txt[^1] == '\r' ? 2 : 1));
                }
                Assert.Equal(txt, lb.ToString());
            }
        }
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void Lines(int i)
        {
            foreach (var s in _steps)
            {
                var count = 0;
                var lb = new LineBuffer(false);
                var ls = _texts[i].SplitLines();
                lb.Subscribe(l =>
                {
                    if (l.Length > 0)
                    {
                        Assert.NotEqual('\n', l.First());
                        Assert.NotEqual('\n', l.Last());
                    }
                    Assert.Equal(l, ls[count], StringComparer.Ordinal);
                    count += 1;
                });
                lb.Bites(_texts[i], s);
                Assert.Equal(ls.Last().IsNullOrEmpty() ? 4 : 5, count);
            }
            
        }
    }
    static class LineBufferTestExtensions
    {
        public static void Bites(this LineBuffer lb, string s, int biteSize)
        {
            while (!s.IsNullOrEmpty())
            {
                if (s.Length <= biteSize)
                {
                    lb.Append(s);
                    break;
                }
                lb.Append(s.Substring(0, biteSize));
                s = s.Substring(biteSize);
            }
            lb.Flush();
        }
    }
}
