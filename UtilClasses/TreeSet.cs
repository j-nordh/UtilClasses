using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;

namespace UtilClasses
{
    public class TreeSet
    {
        private readonly char _separator;
        public string? Name { get; }
        private Dictionary<string, TreeSet> _children = new DictionaryOic<TreeSet>();
        HashSet<string> _leaves = new(StringComparer.OrdinalIgnoreCase);

        public TreeSet(string name, char separator = '.') : this(separator)
        {
            Name = name;
        }

        public TreeSet(char separator = '.')
        {
            _separator = separator;
        }

        public void AddRange(IEnumerable<string> ids) => ids.ForEach(Add);
        public void Add(string id)
        {
            if (!id.Contains(_separator))
            {
                _leaves.Add(id);
                return;
            }
            
            var child = id.Trim(_separator).SubstringBefore(_separator, out var subPath, false);
            _children.GetOrAdd(child, () => new TreeSet(child, _separator)).Add(subPath);
        }

        public bool Contains(string id) => id.Contains(_separator) 
            ? Recurse(id, Contains) 
            : _leaves.Contains(id);

        private static bool Contains(TreeSet ts, string id) => ts.Contains(id);
        private static bool HasNode(TreeSet ts, string id) => ts.HasNode(id);
        public bool HasNode(string id)
        {
            id = id.Trim(_separator);
            return !id.Contains(_separator)
                ? _children.ContainsKey(id)
                : Recurse(id, HasNode);
        }

        private void Recurse(string id, Action<TreeSet, string> a) => Recurse(id, (ts, subId) =>
        {
            a(ts, subId);
            return 0;
        });
        private T? Recurse<T>(string id, Func<TreeSet, string, T> f, T? defaultValue = default)
        {
            var child = GetChild(id, out var subId);
            return null == child ? defaultValue : f(child, subId);
        }
        private TreeSet? GetChild(string id, out string subId)
        {
            var childName = id.SubstringBefore(_separator, out subId, false);
            return _children.Maybe(childName);
        }

        public List<string> All()
        {
            var ret = _leaves.ToList();
            foreach (var kvp in _children)
            {
                ret.AddRange(kvp.Value.All().Select(v => $"{kvp.Key}.{v}"));
            }
            return ret;
        }
        public List<TreeSet> Matching(string id)
        {
            if (!id.StartsWithOic(Name)) return new();
            if (id.EqualsOic($"{Name}{_separator}")) return new() { this };
            var childName = id.SubstringBefore(_separator);
            var child = _children.Maybe(childName);
            if (null == child) return new();
            return child.Matching(id.SubstringAfter(_separator));
        }

        public int Count => _leaves.Count + _children.Values.Sum(c => c.Count);
        public int Depth => _children.Count == 0 ? 1 : _children.Values.Max(c => c.Depth) + 1;

        public override string ToString()
        {
            return GetLines().Join("\n");
        }

        private List<string> GetLines()
        {
            var sb = new IndentingStringBuilder("  ");
            var leafStr = _leaves.Any() 
                ? $" ({_leaves.Join(", ")})" 
                : "";
            sb.AppendLine($"{Name ?? "<root>"}{leafStr}");
            
            var children = _children.Values.OrderBy(ts => ts.Name).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var branch = i == children.Count - 1 ? "┗╸" : "┣╸";
                var carry = i == children.Count - 1 ? "   " : "┃  ";
                var lines = child.GetLines();
                sb.AppendLine($"{branch} {lines.First()}");
                sb.AppendLines(lines.Skip(1).Select(l => $"{carry}{l}"));
            }

            return sb.ToString().SplitLines().NotNullOrWhitespace().ToList();
        }

        public IReadOnlyCollection<TreeSet> Nodes => _children.Values;
        public IReadOnlyCollection<string> Leaves => _leaves;
    }

    public static class TreeSetExtensions
    {
        public static TreeSet AsTreeSet(this IEnumerable<string> values, char separator ='.', string name="")
        {
            var ret = new TreeSet(name, separator);
            ret.AddRange(values);
            return ret;
        }
    }

    public class TreeSetHelper
    {
        private readonly string _basePrefix;

        public TreeSetHelper(string basePrefix)
        {
            _basePrefix = basePrefix;
        }


        public string GetId(IEnumerable<string> parts, int step =0)
        {
            var lst = new List<string>();
            var stepStr = 0 == step ? "" : Enumerable.Repeat('#', step).AsString();
            if(!_basePrefix.IsNullOrWhitespace())
                lst.Add(_basePrefix);
            parts
                .Where(p => !p.IsNullOrWhitespace())
                .Select(p => p.Trim().Trim('.'))
                .Into(lst);
            return $"{lst.Join(".")}{stepStr}";
        }

        public string GetId(string s) => GetId(new[] { s });

        public string GetId(params string[] parts) => GetId(parts, 0);
        public string GetId(int? step, params string[] parts) => GetId(parts, step??0);

        public string GetPrefix(params string[] parts) => $"{GetId(parts, 0)}.";
        public string GetPrefix(int? index ,params string[] parts)
        {
            var indexStr = (index??0) ==0?"":$"[{index}]";
            return $"{GetId(parts, 0)}{indexStr}.";
        }

        public void PrefixAndUpdate<T>(Dictionary<string, T> source, Dictionary<string, T> target, int? index,
            params string[] parts)
        {
            foreach (var kvp in source)
                target[GetId(index, parts.Union(new []{kvp.Key}).ToArray() )] = kvp.Value;
        }

        public IEnumerable<string> Prefix(IEnumerable<string> keys) => keys.Select(GetId);
        public List<string> Prefix(List<string> keys) => keys.Select(GetId).ToList();
        public IEnumerable<string> Prefix(string p, IEnumerable<string> keys) => keys.Select(k=> GetId(p, k));
        public IEnumerable<string> Prefix(string p1,string p2, IEnumerable<string> keys) => keys.Select(k=> GetId(p1, p2, k));
        
    }
}