using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.CodeGeneration
{
    public interface ICodeElement
    {
        string Name { get; }
        IEnumerable<string> Requires { get; }
        void AppendTo(IndentingStringBuilder sb);
    }
}
