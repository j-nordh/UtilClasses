using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses
{
    class SourceFileStringBuilder : IndentingStringBuilder
    {
        public SourceFileStringBuilder(IEnumerable<string> usings, string ns,string declaration) : base("\t")
        {

            
        }
    }
}
