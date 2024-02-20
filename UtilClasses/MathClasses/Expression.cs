using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.MathClasses
{
    public interface IExpression
    {
        decimal? Evaluate(IResolver<string, decimal?> varLookup);
        int Depth { get; }
        Validity IsValid();
        int VarSteps { get; }
        [NotNull]
        List<string> Variables { get; }
    }

    public enum Validity
    {
        Ok,
        Null,
        NotFound,
        ParseError
    }

    public static class ValidityExtensions
    {
        public static Validity Combine(Validity a, Validity b)
        {
            if (a == Validity.ParseError || b == Validity.ParseError) return Validity.ParseError;
            if (a == Validity.NotFound || b == Validity.NotFound) return Validity.NotFound;
            if (a == Validity.Null || b == Validity.Null) return Validity.Null;
            return Validity.Ok;
        }

        public static Validity CombineWith(this Validity a, Validity b) => Combine(a, b);
    }

    public abstract class Expression : IExpression
    {
        public abstract decimal? Evaluate(IResolver<string, decimal?> varLookup);

        public abstract int Depth { get; }


        public abstract Validity IsValid();
        public abstract int VarSteps { get; }
        public abstract Validity IsValid(IResolver<string, decimal?> varLookup);
        public abstract List<string> Variables { get; }

        private static List<string> GetParts(string exp)
        {
            var parts = new List<string>();
            //parenthesis
            int count = 0;
            int start = -1;
            int lastStop = 0;
            for (int i = 0; i < exp.Length; i++)
            {
                var c = exp[i];
                if (c.Equals('('))
                {
                    count += 1;
                    if (start >= 0) continue;
                    start = i;
                    if (i - lastStop > 0) parts.Add(exp.Substring(lastStop, i - lastStop));
                }
                if (c.Equals(')'))
                {
                    count -= 1;
                    if (count > 0) continue;
                    parts.Add(exp.Substring(start + 1, i - start - 1));
                    lastStop = i + 1;
                    start = -1;
                }
            }
            if (parts.Any() && lastStop < exp.Length)
                parts.Add(exp.Substring(lastStop));
            return parts;
        }
        public static Expression Parse(string exp)
        {
            var parts = GetParts(exp);
            if (parts.Any()) // the expression can be split into separate parts before parsing
            {
                var parsed = parts.Select(Parse);
                //var expressions = parsed.Select(p => new ExpressionPart(p)).ToList();
                var chained = parsed.Chain();
                return chained;
            }


            bool inVar = false;
            var current = new StringBuilder();
            var ret = new List<Expression>();
            foreach (var c in exp)
            {
                switch (c)
                {
                    case ' ' when inVar: // ignore space unless it's in a variables name
                        current.Append(c);
                        break;
                    case '[': // variables are denoted by brackets e.g. [variable name]
                        inVar = true;
                        ret.AddConst(current); //whatever is in current is probably a constant
                        break;
                    case ']':
                        ret.Add(new VarTerm(current.ToString()));
                        current.Clear();
                        inVar = false;
                        break;

                    case '*':
                    case '-':
                    case '+':
                    case '/':
                    case '^':
                    case '=':
                    case '>':
                    case '<':
                        if (inVar) //allow variable names to contain operator characters
                        {
                            current.Append(c);
                            break;
                        }
                        ret.AddConst(current); //whatever is in current is probably a constant
                        ret.Add(new OpTerm(c));
                        break;
                    default:
                        current.Append(c);
                        break;
                }
            }
            ret.AddConst(current);//add the last part.
            var chain = ret.Chain();
            return chain;
        }
    }

    public interface IResolver<TIn, out TOut>
    {
        TOut Resolve(TIn key);
        List<TIn> Keys { get; }
        bool ContainsKey(TIn key);
        Task Init(IEnumerable<string> lines, int steps);
        string Filter(string input);
    }
    public static class Resolver
    {
        public static IResolver<TIn, TOut> FromDictionary<TIn, TOut>(Dictionary<TIn, TOut> dict) where TOut : class
            => new DictionaryClassResolver<TIn, TOut>(dict);

        public static IResolver<string, decimal?> FromDictionary(Dictionary<string, decimal> dict)
            => new DictionaryStructResolver<string, decimal>(dict);
        public static IResolver<string, decimal?> FromDictionary(Dictionary<string, decimal?> dict)
            => new DictionaryStructResolver<string, decimal>(dict);
    }

    public class DictionaryStructResolver<TIn, TOut> : IResolver<TIn, TOut?> where TOut : struct
    {
        private readonly Dictionary<TIn, TOut?> _dict;

        public DictionaryStructResolver(Dictionary<TIn, TOut> dict)
        {
            _dict = dict.ToDictionary(kvp => kvp.Key, kvp => (TOut?)kvp.Value, dict.Comparer);
        }
        public DictionaryStructResolver(Dictionary<TIn, TOut?> dict)
        {
            _dict = dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, dict.Comparer);
        }

        public TOut? Resolve(TIn key) => _dict.Maybe(key);
        public List<TIn> Keys => _dict.Keys.ToList();
        public bool ContainsKey(TIn key) => _dict.ContainsKey(key);

        public Task Init(IEnumerable<string> lines, int _) => Task.FromResult(0);
        public string Filter(string input) => input;
    }
    public class DictionaryClassResolver<TIn, TOut> : IResolver<TIn, TOut> where TOut : class
    {
        private readonly Dictionary<TIn, TOut> _dict;

        public DictionaryClassResolver(Dictionary<TIn, TOut> dict)
        {
            _dict = dict;
        }

        public TOut Resolve(TIn key) => _dict.Maybe(key);
        public List<TIn> Keys => _dict.Keys.ToList();
        public bool ContainsKey(TIn key) => _dict.ContainsKey(key);

        public Task Init(IEnumerable<string> lines, int _) => Task.FromResult(0);
        public string Filter(string input) => input;
    }

    public enum Operand
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Pow,
        LessThan,
        Equal,
        GreaterThan
    }

    public abstract class BinaryExpression : Expression
    {
        public abstract Expression Left { get; set; }
        public abstract Expression Right { get; set; }
    }


}
