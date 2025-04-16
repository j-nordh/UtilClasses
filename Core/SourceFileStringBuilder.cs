using System.Collections.Generic;

namespace UtilClasses.Core;

class SourceFileStringBuilder : IndentingStringBuilder
{
    public SourceFileStringBuilder(IEnumerable<string> usings, string ns,string declaration) : base("\t")
    {

            
    }
}