using System;
using System.Collections.Generic;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core;

public class FunctionBuilder : IAppendable
{
    public string Modifier { get; set; }
    public string Returns { get; set; }
    public string Name { get; set; }
    public List<string> Parameters { get; set; }
    public List<Func<IndentingStringBuilder, IndentingStringBuilder>> BodyBuilders { get; set; }

    public bool Inline { get; set; }
    public bool HasObj { get; set; }
    public bool HasObjs { get; set; }

    public FunctionBuilder()
    {
        Modifier = "public";
        Returns = "void";
        Parameters = new List<string>();
        BodyBuilders = new List<Func<IndentingStringBuilder, IndentingStringBuilder>>();
        Inline = false;
    }

    public FunctionBuilder(FunctionBuilder o)
    {
        Modifier = o.Modifier;
        Name = o.Name;
        Returns = o.Returns;
        Parameters = new List<string>(o.Parameters);
        BodyBuilders = new List<Func<IndentingStringBuilder, IndentingStringBuilder>>(o.BodyBuilders);
        Inline = o.Inline;
    }

    public FunctionBuilder Overload()
    {
        var ret = new FunctionBuilder(this);
        ret.BodyBuilders.Clear();
        return ret;
    }

    public IndentingStringBuilder AppendObject(IndentingStringBuilder sb)
    {
        if (Inline)
        {
            return sb.AppendLine($"{Modifier} {Returns} {Name}({Parameters?.Join(", ") ?? ""}) =>")
                .Indent(x => x.Inject(BodyBuilders));
        }
        else
        {
            return sb
                .AppendLines($"{Modifier} {Returns} {Name}({Parameters?.Join(", ") ?? ""})", "{")
                .MaybeAppendLines(HasObjs, "if (null == objs) return null;",
                    "var lst = objs.SmartToList();",
                    "if (!lst.Any()) return lst;")
                .Inject(BodyBuilders)
                .MaybeAppendLine(HasObj, "return obj;")
                .MaybeAppendLine(HasObjs, "return lst;")
                .AppendLine("}");
        }
    }
}