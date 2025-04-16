using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Core;
using UtilClasses.Core.Extensions.Dictionaries;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Strings;
using UtilClasses.Core.Files;

namespace UtilClasses.CodeGen
{
    public class CsProject
    {
        string _path;
        string _content;

        public CsProject(string path)
        {
            _path = path;
            
            if(path.IsNotNullOrEmpty() && File.Exists(path))
            {
                _content = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        public CsProject Add_Compile(params string[] files)
        {
            var dir = Path.GetDirectoryName(_path);
            var fileItems = files.Select(p=>PathUtil.GetRelativePath(p, dir)).Select(p=>(p, $"<Compile Include=\"{p}\" />"));
            if (_content.IsNullOrWhitespace()) return this;
            if (_content.ContainsOic("<TargetFramework>netstandard")) return this;
            var preamble = _content.SubstringBefore("<Compile Include", out var rest);
            var includes = Parse(rest.SubstringBefore("</ItemGroup>", out rest))
                .Union(fileItems)
                .OverwritingToDictionary(p => p.Item1, p => p.Item2);
            _content =
                new IndentingStringBuilder("  ")
                .Append(preamble)
                .Indent(2)
                    .AppendObjects(includes, file => file.Value)
                .Outdent(2)
                .Append(rest.TrimStart())
                .ToString();
            return this;
        }
        private IEnumerable<(string name, string xml)> Parse(string s)
        {
            var q = new Queue<string>(s.SplitLines().Select(l=>l.Trim()));
            string name = null;
            var sb = new StringBuilder();
            while(q.Any())
            {
                var line = q.Dequeue();
                if (line.IsNullOrWhitespace()) continue;
                if (line.StartsWithOic("<Compile") && line.EndsWithOic("/>"))
                {
                    yield return (line.SubstringAfter("Include=\"").SubstringBefore("\""), line);
                    continue;
                };
                if(line.StartsWithOic("<Compile") && line.ContainsOic("Include"))
                {
                    name = line.SubstringAfter("Include=\"").SubstringBefore("\"");
                    sb.AppendLine(line);
                    continue;
                }
                if (null == name) throw new Exception("Parse failed, seems to be inside a \"compile\" without knowing that included file....");
                if(line.ContainsOic("</Compile>"))
                {
                    sb.AppendLine(line);
                    yield return (name, sb.ToString());
                    name = null;
                    sb.Clear();
                    continue;
                }
                sb.AppendLine($"\t{line}");
            }
        }
        public bool Save() => new FileSaver(_path,  _content).SaveIfChanged();
    }
}
