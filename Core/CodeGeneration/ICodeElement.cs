using System.Collections.Generic;

namespace UtilClasses.Core.CodeGeneration;

public interface ICodeElement
{
    string Name { get; }
    IEnumerable<string> Requires { get; }
    void AppendTo(IndentingStringBuilder sb);
}