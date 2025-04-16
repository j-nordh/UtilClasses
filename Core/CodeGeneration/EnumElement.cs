using System.Collections.Generic;

namespace UtilClasses.Core.CodeGeneration;

public class EnumElement : ICodeElement
{
    public string Name { get; set; } = "UNNAMED";
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
        public List<string> Attributes=new();
        public string Name;
        public int Id=0;

        public virtual IndentingStringBuilder AppendObject(IndentingStringBuilder sb) => sb
            .AppendLines(Attributes)
            .Append($"{Name} = {Id}");

        public Member(string name)
        {
            Name = name;
        }

    }
}