using System;
using System.Collections.Generic;
using UtilClasses.Core.Extensions.Objects;

namespace UtilClasses.Core.CodeGeneration;

public class ExtensionClassElement : SimpleClassBuilder
{
    private readonly List<string> _requires;

    public List<Action<IndentingStringBuilder>> Methods { get; }
    public override IEnumerable<string> Requires => _requires;
    public ExtensionClassElement(string name) : base(name + "Extensions")
    {
        IsStatic = true;
        Methods = new List<Action<IndentingStringBuilder>>();
        _requires = new List<string>() {};
    }

    public override IEnumerable<string> ConstructorParameters => new string[0];
    public ExtensionClassElement Add(Action<IndentingStringBuilder> a) => this.Do(() => Methods.Add(a));

    public ExtensionClassElement Using(params string[] reqs) => this.Do(() => _requires.AddRange(reqs));
    protected override void ClassBody(IndentingStringBuilder sb)
    {
        Methods.ForEach(m => sb.Append(m));
    }
}