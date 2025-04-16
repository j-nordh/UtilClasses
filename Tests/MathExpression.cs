using System;
using System.Collections.Generic;
using UtilClasses.Core.Extensions.Decimals;
using UtilClasses.Core.Extensions.Dictionaries;
using UtilClasses.Core.Extensions.Strings;
using UtilClasses.Core.MathClasses;
using Xunit;

namespace UtilClasses.Tests
{
    public class MathExpression
    {
        private Dictionary<string, decimal?> _numberVars = new()
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 },
            { "four", 4 },
            { "five", 5 },
            { "six", 6 },
            { "seven", 7 },
            { "eight", 8 },
            { "nine", 9 }
        };


        [Theory]
        [InlineData("1 + 1", 2)]
        [InlineData("5 - 2", 3)]
        [InlineData("15 / 3", 5)]
        [InlineData("10 * 10", 100)]
        [InlineData("7", 7)]
        [InlineData("5 +(4-3) - 2", 4)]
        public void Basic(string s, decimal expected) => Assert.Equal(expected, Evaluate(s));

        [Theory]
        [InlineData("[one]", 1)]
        [InlineData("[four] + 2", 6)]
        [InlineData("8 + [four]", 12)]
        [InlineData("[two] + [three]", 5)]
        public void Variables(string s, decimal expected) => Assert.Equal(expected,
            Evaluate(s,
                _numberVars));

        [Theory]
        [InlineData("{fyra:four}; [fyra]+1", 5)]
        [InlineData("{tolv:12}; [tolv]+1", 13)]
        [InlineData(@"{tjugo:20}
                      {fyra:four}
                      [tjugo]/[fyra]", 5)]
        [InlineData(@"{tjugo:20}{fyra:four}[tjugo]/[fyra]", 5)]
        [InlineData("[four]+1", 5)]
        [InlineData("{ett:one}; [ett]+[ett#]", 1)]
        public void Aliases(string s, decimal expected)
        {
            _numberVars["one#"] = 0;
            Assert.True(expected.Equals(Evaluate(s, _numberVars)), $"Tried to evaluate {s}.");
        }

        [Theory]
        [InlineData(typeof(FormatException), "1+[apa")]
        public void Throws(Type exceptionType, string s) => Assert.Throws(exceptionType, () => Evaluate(s));

        [Fact]
        public void EcoElectricityUsed()
        {
            var exp = "([L1] + [L2] + [L3])/(3.6*10^9) - ([vpa1] - [vpa1']) - ([vpa2] - [vpa2']) - ([vpa3] - [vpa3'])";
            var vars = new Dictionary<string, decimal?>
    {
        {"L1", 14e9m},
        {"L2", 14.8e9m},
        {"L3", 14.4e9m},
        {"vpa1", 4},
        {"vpa1'", 3},
        {"vpa2", 7},
        {"vpa2'", 5},
        {"vpa3", 9},
        {"vpa3'", 6}
    };
            Assert.Equal(6, Evaluate(exp, vars));
        }

        [Theory]
        [InlineData("(([x]-[x#])^2)+(([y]-[y#])^2)")]
        public void SquareFormula(string expression)
        {
            Assert.Equal(13, Evaluate(expression, "x:4", "x#:2", "y:6", "y#:3"));
        }
        [Theory]
        [InlineData(0, "1")]
        [InlineData(1, "[x]")]
        [InlineData(2, "[x#]")]
        [InlineData(2, "(([x]-[x#])^2)+(([y]-[y#])^2)")]
        [InlineData(25, "[x]-[x########################]")]
        public void VarSteps(int expected, string expression)
        {
            var exp = Expression.Parse(expression);
            Assert.Equal(expected, exp.VarSteps);
        }

        private decimal? Evaluate(string s) => Evaluate(s, _numberVars);

        private decimal? Evaluate(string s, params string[] vars)
        {
            var dict = vars.ToDictionary(v => StringExtensions.SubstringBefore(v, ":"), v => StringExtensions.SubstringAfter(v, ":").MaybeAsDecimal(), StringComparison.OrdinalIgnoreCase);
            return Evaluate(s, dict);
        }
        private decimal? Evaluate(string s, Dictionary<string, decimal?> vars)
        {
            var res = new AliasResolver(vars);
            s = res.Parse(s);
            return Expression.Parse(s).Evaluate(res);
        }
    }

}
