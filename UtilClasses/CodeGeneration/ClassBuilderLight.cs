using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;
using UtilClasses.Files;

namespace UtilClasses.CodeGeneration;

public class ClassBuilderLight
{
    public string Name { get; }
    public string NameSpace { get; set; }
    public bool IsStatic { get; set; }
    public string AccessModifier { get; set; } = "public";
    public List<string> Using { get; } = new();
    public List<string> Props { get; } = new();
    public string ConstructorBody { get; set; }
    public string StaticConstructorBody { get; set; }
    public List<string> ConstructorArgs { get; } = new();
    public List<string> Implements { get; } = new();
    public List<string> Methods { get; } = new();
    public string? Inherits { get; set; }
    public List<string> Attributes { get; } = new();
    public string FileComment { get; set; }
    public string ClassComment { get; set; }

    public DictionaryOic<HandCodedBlock> Blocks { get; } = new()
    {
        ["Methods"] = HandCodedBlock.FromKeyword("Methods"),
        ["Constructor"] = HandCodedBlock.FromKeyword("Constructor")
    };

    public ClassBuilderLight(string name)
    {
        Name = name;
    }

    public override string ToString() => ToString(true);

    public string ToString(bool withUsing)
    {
        var sb = new IndentingStringBuilder("\t").AutoIndentOnCurlyBraces();
        AppendTo(sb, withUsing);
        return sb.ToString();
    }


    public void AppendTo(IndentingStringBuilder sb, bool withUsing)
    {
        var implements = Implements.ToList();
        if (Inherits.IsNotNullOrEmpty())
            implements.Insert(0, Inherits);

        var ii = !implements.Any() ? "" : $": {implements.Join(", ")}";
        sb.AutoIndentOnCurlyBraces()
            .MaybeAppendLines(withUsing, Using.Select(s => $"using {s};"))
            .MaybeAppendLine(FileComment.IsNotNullOrEmpty(), FileComment)
            .AppendLine($"namespace {NameSpace};")
            .MaybeAppendLine(ClassComment.IsNotNullOrEmpty(), ClassComment);

        var baseInitializer = Inherits.IsNullOrEmpty() ? "" : $": base()"; //Future improvement: arguments
        sb.AppendLines(Attributes.Select(a => $"[{a}]"))
            .AppendLine($"{AccessModifier} {(IsStatic ? "static " : "")}class {Name} {ii}")
            .AppendLine("{")
            .AppendLines(Props)
            .AppendLine()
            .Maybe(StaticConstructorBody.IsNotNullOrEmpty(), x=>x
                .AppendLines($"static {Name}()","{", StaticConstructorBody,"}"))
            .Maybe(ConstructorArgs.Any(), () => sb
                .AppendLines($"public {Name}({ConstructorArgs}).Join(", ")}){baseInitializer}",
                    "{")
                .AppendObject(Blocks["Constructor"].Empty())
                .AppendLine("}"))
            .AppendLines(Methods)
            .AppendObject(Blocks["Methods"].Empty())
            .AppendLine("}");
    }
}

public static class ClassBuilderLightExtensions
{

    public static ClassBuilderLight Requiring(this ClassBuilderLight cbl, params string[] args)
    {
        cbl.Using.AddRange(args);
        return cbl;
    }

    public static ClassBuilderLight WithAttribute(this ClassBuilderLight cbl, params string[] args)
    {
        cbl.Attributes.AddRange(args);
        return cbl;
    }

    public static ClassBuilderLight WithNamespace(this ClassBuilderLight cbl, string ns)
    {
        cbl.NameSpace = ns;
        return cbl;
    }
    public static ClassBuilderLight WithFileComment(this ClassBuilderLight cbl, string c)
    {
        cbl.FileComment = c;
        return cbl;
    }
    public static ClassBuilderLight WithProps(this ClassBuilderLight cbl, params string[] fs)
    {
        cbl.Props.AddRange(fs);
        return cbl;
    }
    public static ClassBuilderLight WithMethods(this ClassBuilderLight cbl, params string[] fs)
    {
        cbl.Methods.AddRange(fs);
        return cbl;
    }
    public static ClassBuilderLight WithFields(this ClassBuilderLight cbl, params string[] fs)
    {
        cbl.Props.AddRange(fs);
        return cbl;
    }
    public static ClassBuilderLight WithConstructorBody(this ClassBuilderLight cbl, string body)
    {
        cbl.ConstructorBody = body;
        return cbl;
    }
    public static ClassBuilderLight WithStaticConstructorBody(this ClassBuilderLight cbl, string body)
    {
        cbl.StaticConstructorBody = body;
        return cbl;
    }
    public static ClassBuilderLight Inheriting(this ClassBuilderLight cbl, string baseClass)
    {
        cbl.Inherits = baseClass;
        return cbl;
    }

    public static ClassBuilderLight SetStatic(this ClassBuilderLight cbl, bool isStatic = true)
    {
        cbl.IsStatic = isStatic;
        return cbl;
    }

    public static ClassBuilderLight Implementing(this ClassBuilderLight cbl, params string[] inter)
    {
        if (inter.IsNullOrEmpty())
            return cbl;
        cbl.Implements.AddRange(inter.NotNull());
        return cbl;
    }

    public static ClassBuilderLight Implementing(this ClassBuilderLight cbl, params (string Interface, string Namespace)[] inter)
    {
        if (inter.IsNullOrEmpty())
            return cbl;
        cbl.Implements.AddRange(inter.Select(t => t.Interface).NotNull());
        var namespaces = cbl.Using.Union(inter.Select(t => t.Namespace)).Distinct().OrderBy(s => s).ToList();
        cbl.Using.Clear();
        cbl.Using.AddRange(namespaces);
        return cbl;
    }

    public static ClassBuilderLight MaybeImplementing(this ClassBuilderLight cbl, bool predicate, params string[] inter) => predicate
        ? cbl.Implementing(inter)
        : cbl;
}