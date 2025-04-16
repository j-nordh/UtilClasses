using System.Collections.Generic;

namespace UtilClasses.Core.CodeGeneration;

public interface IInjector
{
    string Name { get; }
    IEnumerable<string> Using { get; }
    IEnumerable<string> Fields { get; }
    IEnumerable<string> ConstructorArgs { get; }
    IndentingStringBuilder Constructor(IndentingStringBuilder sb);
    IndentingStringBuilder Methods(IndentingStringBuilder sb);
    IndentingStringBuilder SubClasses(IndentingStringBuilder sb);
    string Inherits { get; }
    string BaseConstructorArguments { get; }
    IEnumerable<string> Implements { get; }
}