using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses;

namespace UtilClasses.CodeGeneration
{
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
}
