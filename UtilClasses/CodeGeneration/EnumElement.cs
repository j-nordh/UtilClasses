using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.CodeGeneration
{
    public class EnumElement : ICodeElement
    {
        public string Name { get; set; }
        public string Modifier { get; set; } = "public";
        public List<Member> Members { get; set; } = new List<Member>();
        public List<string> Requires { get; set; }

        IEnumerable<string> ICodeElement.Requires => Requires;

        public void AppendTo(IndentingStringBuilder sb)
        {   
            
            sb.AppendLines($"{Modifier} enum {Name}", "{")
                .AppendObjects(Members, ",")
                .AppendLine("}");
        }

        public EnumElement()
        {
            Requires = new List<string>();
        }

        public class Member:IAppendable
        {
            public List<string> Attributes;
            public string Name;
            public int Id;

            public virtual IndentingStringBuilder AppendObject(IndentingStringBuilder sb) => sb
                .AppendLines(Attributes)
                .Append($"{Name} = {Id}");

            public Member()
            {
                Attributes = new List<string>();
            }

        }
    }
}
