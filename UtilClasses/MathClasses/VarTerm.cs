using System;
using System.Collections.Generic;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.MathClasses
{
    public class VarTerm : Expression
    {
        public string Name { get; }
        public override Validity IsValid(IResolver<string, decimal?> varLookup)
        {
            var valid = IsValid();
            if (valid != Validity.Ok) return valid;
            decimal? ret = null;
            try
            {
                ret = varLookup.Resolve(Name);
            }
            catch (KeyNotFoundException)
            {
                return Validity.NotFound;
            }

            return null == ret ? Validity.Null : Validity.Ok;
        }

        public override int Depth => 1;

        public override List<string> Variables { get; } = new();

        public VarTerm(string name)
        {
            Name = name;
        }
        public override Validity IsValid() => Name.IsNullOrEmpty()? Validity.ParseError : Validity.Ok;
        public override int VarSteps => Name.CountOic("#")+1;

        public override decimal? Evaluate(IResolver<string, decimal?> varLookup)
        {
            return varLookup.Resolve(Name);
        }

        public override string ToString() => $"[{Name}]";
    }
}