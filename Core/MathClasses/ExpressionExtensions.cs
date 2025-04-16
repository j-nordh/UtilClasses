using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.MathClasses;

public static class ExpressionExtensions
{
    public static Expression Chain(this IEnumerable<Expression> parts)
    {
        if (null == parts ) return null;
        var lst = parts.ToList();
        if (!lst.Any()) return null;
            
        while (true)
        {
            var c = lst.Count;
            lst = lst
                .Chain(Operand.Pow)
                .Chain(Operand.Multiply, Operand.Divide)
                .Chain(Operand.Add, Operand.Subtract);
            if(c==lst.Count) break;
        }
        if (lst.Count() == 1) return lst.Single();
        throw new ArgumentException($"The expression {lst.Select(p=>p.ToString()).Join(" ")} could not be normalized");
    }
    //public static decimal Evaluate(this IExpression ex, Dictionary<string, decimal> dict) =>
    //    ex.Evaluate(Resolver.FromDictionary(dict));

    private static List<Expression> Chain(this List<Expression> parts, params Operand[] ops)
    {
        var stack = new Stack<Expression>();

        foreach (var p in parts)
        {
            var current = p as OpTerm;
            if (stack.Any() &&stack.Peek() is OpTerm {Right: null} prev && ops.Contains(prev.Op))
            {   //is there an expression on top of the stack that needs a right term?
                prev.Right = p;
                continue;
            }
            if (null == current || !ops.Contains(current.Op))
            {   //is the part either not an operator or an operator that we don't cara about this execution?
                stack.Push(p);
                continue;
            }

            if (current.Left == null && stack.Any()) current.Left = stack.Pop(); //is the current operator missing a left term that we could fetch from the stack?
            stack.Push(current);
        }

        return stack.Reverse().ToList();
    }

       
    public static List<Expression> AddConst(this List<Expression> lst, string s)
    {
        if (!s.Trim().Any()) return lst;
        lst.Add(ConstTerm.Parse(s));
        return lst;
    }
    public static List<Expression> AddConst(this List<Expression> lst, StringBuilder sb)
    {
        lst.AddConst(sb.ToString());
        sb.Clear();
        return lst;
    }
}