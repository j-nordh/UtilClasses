using UtilClasses.Extensions.Strings;
using Xunit;

namespace UtilClasses.Tests;

public class StringExtensionTests
{
    [Fact]
    public void MaybeGetLine()
    {
        string? s = null;
        Assert.Null(s.MaybeGetLine(0));
        Assert.Null(s.MaybeGetLine(2342));

        s = """
            a

            b
            """;
        Assert.Equal("a", s.MaybeGetLine(0));
        Assert.Equal("", s.MaybeGetLine(1));
        Assert.Equal("b", s.MaybeGetLine(2));
        Assert.Null(s.MaybeGetLine(4));
        Assert.Null(s.MaybeGetLine(-1));
        
        Assert.Equal("a", s.MaybeGetLine(0, true));
        Assert.Equal("b", s.MaybeGetLine(1, true));
    }
}