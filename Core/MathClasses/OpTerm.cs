using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilClasses.Core.MathClasses;

public class OpTerm : BinaryExpression
{
    public override Expression Left { get; set; }
    public override Expression Right { get; set; }
    public Operand Op => _spec.Op;
    private OpSpec _spec;

    private static List<OpSpec> _ops;
    private static Dictionary<char, OpSpec> _charLookup;
    private static Dictionary<Operand, OpSpec> _opLookup;

    public override List<string> Variables => Left.Variables.Union(Right.Variables).ToList();
    static OpTerm()
    {
        _ops = new List<OpSpec>
        {
            {'+', Operand.Add, (l,r)=>l+r },
            {'-', Operand.Subtract, (l,r)=>l-r },
            {'*', Operand.Multiply, (l,r)=>l*r },
            {'/', Operand.Divide, (l,r)=>l/r },
            {'^', Operand.Pow, (l,r)=>(decimal)Math.Round(Math.Pow((double)l,(double)r),6)},
            {'>', Operand.GreaterThan, (l,r)=>l>r?1:0 },
            {'<', Operand.LessThan, (l,r)=>l<r?1:0  },
            {'=', Operand.Equal, (l,r)=>l==r?1:0  },
        };

        _charLookup = _ops.ToDictionary(os => os.C);
        _opLookup = _ops.ToDictionary(os => os.Op);
    }

    public override decimal? Evaluate(IResolver<string, decimal?> varLookup)
    {
        if (null == Left && Operand.Subtract == Op)
            Left = new ConstTerm(0);
        var l = Left?.Evaluate(varLookup);
        var r = Right.Evaluate(varLookup);
        return _spec.Perform(l, r);
    }

    public override Validity IsValid(IResolver<string, decimal?> varLookup)
    {
        if (null == Left && null == Right)
            return Validity.Null;
            
        if (null == Left)
        {
            if (Operand.Subtract == Op ) 
                Left = new ConstTerm(0);
        }
            
        return Left!.IsValid(varLookup).CombineWith(Right.IsValid(varLookup));
    }

    public override int Depth => new[] { Left?.Depth ?? 0, Right?.Depth ?? 0 }.Max() + 1;

    public override Validity IsValid()
    {
        if (Left == null && Op != Operand.Subtract || Right == null)
            return Validity.ParseError;
        var l = Left?.IsValid() ?? Validity.Ok;
        return l.CombineWith(Right.IsValid());
    }

    public override int VarSteps => new[] { Left?.VarSteps ?? 0, Right?.VarSteps ?? 0 }.Max();


    public OpTerm(Operand op) => _spec = _opLookup[op];
    public OpTerm(char c) => _spec = _charLookup[c];


    public override string ToString() => $"{Left} {_spec.C} {Right}";
}