using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Objects;

namespace UtilClasses
{
    public class UsageFormatter
    {
        private readonly string _name;
        public List<(string name, string description, bool required)> Parameters { get; private set; }
        public Dictionary<string, List<(string name, string description, bool required)>> Sections {get; private set; }

        public UsageFormatter(string name, IEnumerable<(string name, string description, bool required)>? parameters = null)
        {
            _name = name;
            Parameters = parameters.SmartToList();
            IndentString = "\t";
            Sections = new DictionaryOic<List<(string name, string description, bool required)>>();
        }

        public string IndentString { get; set; }
        public bool Sort { get; set; }

        public override string ToString()
        {
            if (Sort)
            {
                Parameters = Parameters.OrderBy(x => !x.required).ThenBy(x => x.name).ToList();
            }
            var width = _name.Length + 10;
            var frame = string.Concat(Enumerable.Repeat('"', width));
            var sb = new IndentingStringBuilder(IndentString);

            void RenderParameter((string name, string description, bool required) valueTuple)
            {
                var opt = valueTuple.required ? "" : " (optional)";
                sb.AppendLine($"{valueTuple.name}{opt}")
                    .Indent(() => sb.AppendLine(valueTuple.description));
            }

            sb
                .AppendLines(frame, $"#    {_name}    #", frame)
                .AppendObjects(Parameters, RenderParameter)
                .AppendObjects(Sections, s =>
                {
                    sb.AppendLine($"-- {s.Key} --")
                        .AppendObjects(s.Value, RenderParameter);
                });
            return sb.ToString();
        }
    }

    public static class UsageFormatterExtensions
    {
        public static UsageFormatter Sorted(this UsageFormatter uf, bool sort = true) => uf.Do(() => uf.Sort = sort);

        public static UsageFormatter With(this UsageFormatter uf, string name, string description,
            bool required = false) => uf.Do(() => uf.Parameters.Add((name, description, required)));

        public static UsageFormatter With(this UsageFormatter uf, (string, string, bool) p) =>
            uf.Do(() => uf.Parameters.Add(p));
        public static UsageFormatter With(this UsageFormatter uf, IEnumerable<(string, string, bool)> ps) =>
            uf.Do(() => uf.Parameters.AddRange(ps));
        public static UsageFormatter InSection(this UsageFormatter uf, string section, IEnumerable<(string, string, bool)> ps)=>
            uf.Do(()=> uf.Sections.GetOrAdd(section).AddRange(ps));
    }
}
