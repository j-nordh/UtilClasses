using System;
using System.Collections.Generic;

namespace UtilClasses.Core.MathClasses;

class OpSpec
{
    public Operand Op { get; }
    public char C { get; }

    private readonly Func<decimal?, decimal?, decimal?> _performer;
    public decimal? Perform(decimal? left, decimal? right) => _performer(left, right);

    public OpSpec(char c, Operand op, Func<decimal?, decimal?, decimal?> performer)
    {
        C = c;
        Op = op;
        _performer = performer;
    }
}
static class OpSpecExtensions
{
    public static void Add(this List<OpSpec> lst, char c, Operand op, Func<decimal?, decimal?, decimal?> performer)
    {
        lst.Add(new OpSpec(c, op, performer));
    }

}