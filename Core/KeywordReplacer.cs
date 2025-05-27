using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core;

public class KeywordReplacer
{
    private readonly string _startMarker;
    private readonly string _endMarker;
    private readonly StringComparison _comp;
    private readonly Dictionary<string, Func<string?>> _replacements;
    public KeywordReplacer() : this("%", "%", StringComparison.OrdinalIgnoreCase) { }

    public KeywordReplacer(string startMarker, string endMarker, StringComparison comp)
    {
        _startMarker = startMarker;
        _endMarker = endMarker;
        _comp = comp;
        _replacements = new Dictionary<string, Func<string?>>(comp.ToEqualityComparer());
    }

    public static KeywordReplacer Create(string keyword, string value)=>new KeywordReplacer().Add(keyword,value);

    public KeywordReplacer Add(string keyword, string? value)
    {
        _replacements[keyword] = () => value;
        return this;
    }
    public KeywordReplacer Add(string keyword, Func<string?> f)
    {
        _replacements[keyword] = f;
        return this;
    }
    public KeywordReplacer Add(IEnumerable<string> keywords, Func<string, string?> f)
    {
        foreach (var keyword in keywords)
        {
            _replacements[keyword] = ()=>f(keyword);
        }
        
        return this;
    }
    public KeywordReplacer Add(string keyword, object value) => Add(keyword, value.ToString());

    public KeywordReplacer Add(string keyword, IEnumerable<string> lines)
    {
        _replacements[keyword] = () => lines.Join("\r\n");
        return this;
    }

    public KeywordReplacer Add(IEnumerable<KeyValuePair<string, string?>> kvps)
    {
        foreach (var kvp in kvps)
        {
            Add(kvp.Key, kvp.Value);
        }
        return this;
    }
    public KeywordReplacer Add(IEnumerable<(string? Keyword, string? Value)>? tuples)
    {
        if (tuples == null)
            return this;
        foreach (var t in tuples)
        {
            if(t.Keyword == null)
                continue;
            Add(t.Keyword, t.Value);
        }
        return this;
    }
    

    public KeywordReplacer Add<T>(string keyword, IEnumerable<T> items, Func<T, string> f, string separator = "\r\n")
    {
        Add(keyword, items.Select(f).Join(separator));
        return this;
    }

    public string Run(string? input)
    {
        if (input.IsNullOrEmpty())
            return "";
        var sb = new StringBuilder();
        int i = 0;
        while (i < input!.Length)
        {
            var tagStart = input.IndexOf(_startMarker, i, _comp);
            if (tagStart == -1)
            {
                sb.Append(input.Substring(i));
                break;
            }
            sb.Append(input.Substring(i, tagStart - i));
            i = tagStart;

            var tagEnd = input.IndexOf(_endMarker, tagStart + 1, _comp);
            if (tagEnd == -1)
            {
                sb.Append(input.Substring(i));
                break;
            }
            var tag = input.Substring(tagStart + 1, tagEnd - tagStart - 1).Trim();
            if (tag.Length > 30 || tag.Contains("\n"))
            {
                sb.Append(input.Substring(i, tagEnd - i));
                i = tagEnd;
                continue;
            }
            sb.Append(_replacements.TryGetValue(tag, out var replacement)
                ? Run(replacement())
                : input.Substring(tagStart, tagEnd + 1 - tagStart));
            i = tagEnd + 1;
        }
        return sb.ToString();
    }
}