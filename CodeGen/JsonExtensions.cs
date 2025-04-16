using Newtonsoft.Json;
using SupplyChain.Procs;
using System.Linq;
using SupplyChain.Dto;
using UtilClasses.Core;

namespace UtilClasses.CodeGen
{
    public static class JsonExtensions
    {
        
        public static IndentingStringBuilder Append(this IndentingStringBuilder sb, ClassDef c)
            => sb.AppendLine("{").Indent()
                .AppendLines(
                    $"\"ClassName\": \"{c.ClassName}\",",
                    "\"Procedures\": [")
                .AppendObjects(c.Procedures, p=>Append(sb,p), ",", false)
                .Outdent()
                .AppendLine("]").Append("}");
        private static IndentingStringBuilder Append(this IndentingStringBuilder sb, ProcDef p)
        {
            sb.AppendLine("{").Indent()
                .AppendLines($"\"SpName\": \"{p.SpName}\",",
                $"\"Name\": \"{p.Name}\",")
                .Append("\"Calls\" :[");
            if (p.Calls.Count > 1)
            {
                sb.Indent().AppendLine();
            }
            sb.AppendObjects(p.Calls, JsonConvert.SerializeObject, p.Calls.Count() > 1 ? ",\r\n" : ", ", false);
            if (p.Calls.Count > 1)
                sb.Outdent();
            return sb.AppendLine("]")
                .Outdent()
                .Append("}");
        }
    }

}
