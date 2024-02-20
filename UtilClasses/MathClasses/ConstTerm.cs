using System;
using System.Collections.Generic;
using System.Globalization;

namespace UtilClasses.MathClasses
{
    public class ConstTerm : Expression
    {
        public ConstTerm(decimal value)
        {
            Value = value;
        }
        public override int Depth => 1;
        public decimal Value { get; }
        public override Validity IsValid() => Validity.Ok;
        public override int VarSteps => 0;
        public override Validity IsValid(IResolver<string, decimal?> varLookup) => Validity.Ok;

        public override List<string> Variables => new();
    

        public new static ConstTerm Parse(string s)
        {

            var sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            s = s.Replace(",", sep)
                .Replace(".", sep);
            return new ConstTerm(decimal.Parse(s));
        }
        public override decimal? Evaluate(IResolver<string, decimal?> varLookup)
        {
            return Value;
        }
        public override string ToString() => $"{Value}";
    }
}