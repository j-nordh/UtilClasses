using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.CodeGeneration;

public class EnumBuilder:IAppendable
{
    public class Member
    {
        public string Name { get; set; }
        public string? Key { get; set; }
        public int? Value { get; set; }
        public Member(string name)
        {
            Name = name;
        }
        public override string ToString()
        {
            var val = Value.HasValue ? $" = {Value}" : "";
            var ret = "";
            if (Key.IsNotNullOrEmpty())
                ret = $"[Key(\"{Key}\")]\n";
            ret += $"{Name}{val}";
            return ret;
        }
    }
    public string Name { get; set; }
    public string NameSpace { get; set; }
    public string AccessModifier { get; set; } = "public";
    public List<string> Attributes { get; } = new();
    public List<Member> Members { get; } = new();
    public bool Flags { get; set; }

    public EnumBuilder(string name, string ns)
    {
        Name = name;
        NameSpace = ns;
    }
    public static EnumBuilder Parse(string str, string ns)
    {
        var name = str.SubstringBefore("(").SubstringAfter("TYPE ").SubstringBefore(" AS").SubstringAfter(".");
        name = StringUtil.SnakeToCamel(name);
        var parts = str.SubstringAfter("(").SubstringBefore(")").Split(',').Trim().ToList();
        var ret = new EnumBuilder(name ,ns);
        foreach(var part in parts)
        {
            if (!part.ContainsOic("'"))
                continue;
            var m = StringUtil.SnakeToCamel(part.SubstringAfter("'").SubstringBefore("'"));
            ret.Members.Add(m);
        }
        return ret;
    }
    public override string ToString() => new IndentingStringBuilder("    ")
        .AutoIndentOnCurlyBraces()
        .AppendObject(this)
        .ToString();
    public IndentingStringBuilder AppendObject(IndentingStringBuilder sb)
    {
        var attr = Attributes.Select(a => $"[{a}]").ToList();
        if (Flags)
        {
            attr.Add("[Flags]");
            throw new Exception("orka");
        }

        sb.AppendLines(attr)
            .AppendLine($"namespace {NameSpace};")
            .AppendLine($"{AccessModifier} enum {Name}")
            .AppendLine("{")
            .AppendLine(Members.Select(m => m.ToString()).Join(",\n"))
            .AppendLine("}");
        return sb;
    }
}
public static class EnumBuilderExtensions
{
    public static void Add(this List<EnumBuilder.Member> list, string name)
    {
        list.Add(new EnumBuilder.Member(name));
    }
}