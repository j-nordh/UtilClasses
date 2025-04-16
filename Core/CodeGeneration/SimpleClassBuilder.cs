using System.Collections.Generic;
using System.Linq;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.CodeGeneration;

public abstract class SimpleClassBuilder : ICodeElement
{
    protected SimpleClassBuilder(string name)
    {
        Name = name;
    }

    public string Name { get; }
    protected virtual string AccessModifier => "public";
    public virtual IEnumerable<string> Requires => new string[] { };
    public virtual IEnumerable<string> Implements => new string[] { };
    public abstract IEnumerable<string> ConstructorParameters { get; }
    protected bool IsStatic { get; set; }
    protected virtual string Suffix => "";

    public virtual string? BaseConstructorParameters => null;

    public void AppendTo(IndentingStringBuilder sb)
    {
        if (Name.IsNullOrEmpty()) return;
        var implements = (Implements?.Any()??false) ? ": " + Implements.Join(", ") : "";
        var baseCall = BaseConstructorParameters.IsNullOrEmpty() ? "" : $" : base({BaseConstructorParameters})";
        var stat = IsStatic ? "static " : "";
        sb.AppendLines($"{AccessModifier} {stat}class {Name}{Suffix}{implements}", "{")
            .Append(Preamble)
            .Maybe(!IsStatic, ()=>sb
                .AppendLines($"public {Name}{Suffix}({ConstructorParameters.Join(", ")}){baseCall}", "{")
                .Append(ConstructorBody)
                .AppendLine("}"))
            .Append(ClassBody)
            .AppendLine("}");
    }
    protected virtual void Preamble(IndentingStringBuilder sb)
    { }
    protected virtual void ConstructorBody(IndentingStringBuilder sb)
    { }
    protected virtual void ClassBody(IndentingStringBuilder sb)
    { }
}