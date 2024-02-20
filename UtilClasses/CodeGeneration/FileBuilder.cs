using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Files;

namespace UtilClasses.CodeGeneration
{
    public class FileBuilder : IAppendable
    {
        private List<ICodeElement> _elements = new List<ICodeElement>();
        public IEnumerable<string> HeaderSteps { get; set; }
        public HandCodedBlock UsingBlock { get; set; }
        public override string ToString()
        {
            var sb = new IndentingStringBuilder("\t");
            AppendObject(sb);
            return sb.ToString();
        }



        public FileBuilder Add(ICodeElement ce)
        {
            _elements.Add(ce);
            return this;
        }

        public FileBuilder Add(IEnumerable<ICodeElement> ces)
        {
            _elements.AddRange(ces);
            return this;
        }

        public string Namespace { get; set; }
        private IEnumerable<string> GetHeader()
        {
            if (null == HeaderSteps) return new string[] { };
            var q = new Queue<string>(HeaderSteps.Select(s => $"// * {s}"));
            while (q.Count < 3) q.Enqueue("//");
            var lines = new List<string>() {
                @"//                                                              ____            ",
                @"// This file has been generated using SupplyChain.             /\' .\    _____  ",
                @"// Please do not modify it directly, instead:                 /: \___\  / .  /\ ",
                $@"{q.Dequeue(),-62                                            }\' / . / /____/..\",
                $@"{q.Dequeue(),-62                                            } \/___/  \'  '\  /",
                $@"{q.Dequeue(),-62                                            }          \'__'\/ "
            };
            while (q.Any()) lines.Add(q.Dequeue());
            return lines;
        }

        public IndentingStringBuilder AppendObject(IndentingStringBuilder sb)
        {
            sb.AutoIndentOnCurlyBraces();
            var requires = _elements.SelectManyStripNull(i => i.Requires).AsSorted().Distinct().ToList();
            sb
                .AppendLines(requires.Select(n => $"using {n};"))
                .AppendObject(UsingBlock?.Empty())
                .AppendLine()
                .AppendLines(GetHeader())
                .AppendLines()
                .AppendLine($"namespace {Namespace}")
                .AppendLine("{");
            foreach (var e in _elements)
            {
                e.AppendTo(sb);
            }
            return sb.AppendLine("}");
        }
    }
}
