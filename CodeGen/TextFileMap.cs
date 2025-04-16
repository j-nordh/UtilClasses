using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SupplyChain.Dto;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;
using UtilClasses.Files;

namespace UtilClasses.CodeGen
{
    public class TextFileMap
    {
        private readonly CodeEnvironment _env;
        private Dictionary<string, NamespaceInfo> _dict;
        private readonly Dictionary<string, NamespaceInfo> _nsLookup;
        private PathUtil _pathUtil;


        public TextFileMap(CodeEnvironment env)
        {
            _env = env;
            _dict = new Dictionary<string, NamespaceInfo>();
            _nsLookup = new Dictionary<string, NamespaceInfo>();
            NamespaceInfo current = null;
            _pathUtil = new PathUtil(env.Dto.NamespaceMap);
            if (File.Exists(env.Dto.NamespaceMap))
            {
                foreach (var line in File.ReadAllLines(env.Dto.NamespaceMap))
                {
                    if (line.IsNullOrWhitespace()) continue;
                    if (!char.IsWhiteSpace(line[0]))
                    {
                        current = new NamespaceInfo(line);
                        _nsLookup[current.Namespace] = current;
                        continue;
                    }
                    _dict[line.Trim()] = current;
                }
            }
            Save();
        }

        public string GetAbsolutePath(string path) => _pathUtil.GetAbsolutePath(path);
        public string GetNamespace(string type) => Get(type)?.Namespace;
        public string GetDllPath(string type) => Get(type)?.Dll;

        public  NamespaceInfo Get(string type) => type == null ? null : (_dict.Maybe(type) ?? _dict.Maybe(type.StripAllGenerics()));

        public List<NamespaceInfo> All => _dict.Values.ToList();

        public bool Contains(string type) => _dict.ContainsKey(type);
        public TextFileMap Set(string type, string ns) => this.Do(() => _dict[type] = _nsLookup[ns]);
        

        public bool Save() => new FileSaver(_env.Dto.NamespaceMap, ToString()).SaveIfChanged();

        public override string ToString()
        {
            var output = new Dictionary<string, List<string>>();
            foreach (var kvp in _dict)
            {
                output.GetOrAdd(kvp.Value.ToString(), () => new List<string>()).Add(kvp.Key);
            }
            var sb = new IndentingStringBuilder("  ");
            foreach (var ns in output.Keys.AsSorted())
            {
                sb.AppendLine(ns)
                    .Indent()
                    .AppendLines(output[ns].AsSorted())
                    .Outdent();
            }
            return sb.ToString();
        }
    }

    public class NamespaceInfo
    {
        public string Namespace { get; }
        public string Dll { get; }

        public NamespaceInfo(string line)
        {
            if (!line.Contains("|"))
            {
                Namespace = line;
                Dll = null;
                return;
            }
            var parts = line.Split('|').Select(s => s.Trim()).ToList();
            Namespace = parts[0];
            Dll = parts[1];
        }

        public override string ToString()
            => Namespace + (Dll.IsNotNullOrEmpty() ? " | " + Dll : "");
    }
}
