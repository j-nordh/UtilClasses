using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Dictionaries;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.MathClasses;

public class AliasResolver: IResolver<string, decimal?>
{
    private IResolver<string, decimal?> _resolver;
    private readonly IDictionary<string, string> _aliases = new DictionaryOic<string>();

    public AliasResolver(){}

    public AliasResolver(IResolver<string, decimal?> res)
    {
        _resolver = res;
    }

    public AliasResolver(Dictionary<string, decimal?> res)
    {
        _resolver = Resolver.FromDictionary(res);
    }
        
        
    public string Parse(string str)
    {
        var ret = new List<string>();
        foreach (var token in Tokenize(str))
        {
            if (!token.ContainsOic(":"))
            {
                ret.Add(token);
                continue;
            }
            var parts = token.RemoveAllOic("{", "}", ";").Trim().SplitAndTrim(":").ToList();
            if (parts.Count != 2)
                throw new ArgumentException($"The token {token} could not be parsed into \"key:value\"");
            var id = null == CurrentId ? "" : $"{CurrentId}_";
            _aliases.Add($"{id}{parts[0]}",parts[1]);
        }

        return ret.Join("\n");
    }

    public void Init(Dictionary<string, decimal?> dict)
    {
        _resolver = Resolver.FromDictionary(dict);
    }
    public void Init(IResolver<string, decimal?>res )
    {
        _resolver = res;
    }

    public void Init(IEnumerable<(long id, string str)> items, int steps)
    {
        foreach (var item in items)
        {
            CurrentId = item.id;
            Parse(item.str);
            CurrentId = null;
        }

        _resolver.Init(_aliases.Values.Distinct(), steps);
    }

    public void Clear()
    {
        _aliases.Clear();
    }
    private static List<string> Tokenize(string str)
    {
        var tokens = str.RemoveAll("\r")
            .ConfigureTokenizer()
            .AddEnvelope('{', '}')
            .AddDelimiter('\n')
            .AddDelimiter(';')
            .AddDelimiter('}')
            .Run()
            .NotNullOrWhitespace()
            .ToList();
        return tokens;
    }
    public decimal? Resolve(string key)
    {
        if (null == _resolver)
            throw new Exception("No resolver specified. Please call Init().");
        var orgKey = key;
        var resolvedKey = ResolveKey(key);
        return decimal.TryParse(resolvedKey, out var val) 
            ? val 
            : _resolver.Resolve(resolvedKey ?? orgKey);
    }

    public bool ContainsKey(string key) => _resolver.ContainsKey(key) ||
                                           _aliases.ContainsKey(key) && _resolver.ContainsKey(_aliases[key]); 

    public List<string> Keys
    {
        get
        {
            var reverse =
                _aliases.ToDictionary(
                    kvp => kvp.Value, 
                    kvp => kvp.Key, 
                    StringComparison.OrdinalIgnoreCase);
            return _resolver.Keys.Select(k => reverse.Maybe(k) ?? k).ToList();
        }
    }
    public string Aliases => _aliases.Select(kvp=> $"{kvp.Key} => {kvp.Value}").Join("\n");

    public async Task Init(IEnumerable<string> lines, int steps)
    {
        var rest = lines.Select(Parse).NotNull().NotNull();
        if(null == CurrentId)
            await _resolver.Init(_aliases.Values.Union(rest), steps);
    }
    public async Task Init(Dictionary<long, string> expressions, int steps)
    {
        var rest = new List<string>();
        foreach (var kvp in expressions)
        {
            CurrentId = kvp.Key;
            rest.Add(Parse(kvp.Value));
            CurrentId = null;
        }
        await _resolver.Init(rest.Union(_aliases.Values), steps);
    }
    public long? CurrentId { get; set; }

    public string Filter(string input) => _resolver.Filter(Tokenize(input).Where(l => !l.ContainsOic(":")).Join("\n"));

    public string ResolveKey(string key)
    {
        var prims = key.Count(c => c == '#');
        key = key.Trim('#');
        if (null != CurrentId)
            key = $"{CurrentId}_{key}";
            
        var alias = _aliases.Maybe(key) ?? key;
        if (prims > 0)
            alias += Enumerable.Repeat('#', prims).AsString();
        return alias;
    }
}