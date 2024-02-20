using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Db.Extensions
{
    public static class JsonExtensions
    {
        public static string ToXml(this string json, string root, Dictionary<string,string> childNames=null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<{root}>");
            var token = JToken.Parse(json);

            token.WriteTo(sb,1, root, childNames);
            
            sb.AppendLine($"</{root}>");
            return sb.ToString();
        }

        private static void WriteTo(this JToken token, StringBuilder sb, int indent, string parentName, Dictionary<string, string> childNames)
        {
            if (TryPerform<JObject>(token, obj =>
            {
                foreach (var child in obj.Children())
                {
                    child.WriteTo(sb, indent + 1, parentName, childNames);
                }
            })) return;

            if (TryPerform<JProperty>(token, prop =>
            {
                sb.Append(new string(' ', indent));
                sb.Append($"<{prop.Name}>");
                prop.Value.WriteTo(sb, indent + 1, prop.Name,childNames);
                sb.AppendLine($"</{prop.Name}>");
            })) return;

            if (TryPerform<JValue>(token, val =>
            {
                decimal dec;
                sb.Append(decimal.TryParse(val.Value.ToString(), out dec)
                    ? dec.ToString(NumberFormatInfo.InvariantInfo)
                    : val.Value.ToString().EscapeForXml());
            })) return;

            if (TryPerform<JArray>(token, arr =>
            {

                var name = parentName;
                if (childNames?.ContainsKey(parentName) ?? false) name = childNames[parentName];
                else if (parentName.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
                    name = parentName.Substring(0, parentName.Length - 1);
                sb.AppendLine();
                foreach (var item in arr.Children())
                {

                    sb.Append(new string(' ', indent));
                    sb.AppendLine($"<{name}>");
                    item.WriteTo(sb, indent + 1, name, childNames);
                    sb.Append(new string(' ', indent));
                    sb.AppendLine($"</{name}>");
                }
                sb.Append(new string(' ', indent - 1));
            })) return;
            throw new Exception($"Could not write a {token.GetType().Name}");
        }
        private static bool TryPerform<T>(JToken token, Action<T> action) where T : JToken
        {
            var obj = token as T;
            if (null == obj) return false;
            action(obj);
            return true;
        }
        /*
         * "   &quot;
'   &apos;
<   &lt;
>   &gt;
&   &amp;
         */

        class JsonGenTree
        {
            public string Name { get; }
            public JsonGenTree Parent { get; }
            public string Value { get; set; }
            public List<JsonGenTree> Children { get; }

            public JsonGenTree() :this("ROOT", null) { }
            public JsonGenTree(string name, JsonGenTree parent)
            {
                Name = name;
                Parent = parent;
                Value = null;
                Children = new List<JsonGenTree>();
            }

            public override string ToString()
            {
                var sb = new ComplexStringBuilder();
                WriteTo(sb);
                return sb.ToString();
            }

            public bool IsArray
            {
                get
                {
                    if (!Children.Any()) return false;
                    var firstName = Children.First().Name;
                    if (Name.Length - firstName.Length != 1) return false;
                    if (!Name.StartsWith(firstName)) return false;
                    return Children.All(c => c.Name == firstName);
                }
            }

            private void WriteTo(ComplexStringBuilder sb, bool suppressName = false)
            {
                if (Parent == null)
                {
                    if (Children.Count == 1)
                    {
                        Children.First().WriteTo(sb, true);

                        return;
                    }
                }
                if (!suppressName) sb.Append('"').Append(Name).Append("\" : ");
                if (Children.Count == 0)
                {
                    decimal d;
                    string quote = "\"";
                    if (decimal.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) quote = "";
                    sb.Append(quote).Append(Value).Append(quote);
                    return;
                }
                if (IsArray)
                {
                    sb.AppendLine("[");
                    sb.StartItemization();

                    foreach (var child in Children)
                    {
                        var childSb = sb.BeginItem().GetIndented();
                        string wrapper = child.IsArray ? "[]" : "{}";
                        childSb.AppendLine(wrapper.Substring(0, 1));
                        childSb.StartItemization();
                        foreach (var grandChild in child.Children)
                        {
                            childSb.BeginItem();
                            grandChild.WriteTo(childSb.GetIndented());
                            childSb.EndItem();
                        }
                        childSb.EndItemization().AppendLine();
                        childSb.AppendLine(wrapper.Substring(1, 1));
                        sb.EndItem();
                    }
                    sb.EndItemization().AppendLine();
                    sb.AppendLine("]");
                }
                else
                {
                    sb.AppendLine("{");
                    sb.StartItemization();
                    foreach (var child in Children)
                    {

                        child.WriteTo(sb.BeginItem().GetIndented());
                        sb.EndItem();
                    }
                    sb.EndItemization().AppendLine();
                    sb.AppendLine("}");
                }
            }

            public static JsonGenTree FromXml(string xml)
            {
                var tree = new JsonGenTree();
                tree.Read(xml);
                return tree;
            }

            public void Read(string xml)
            {
                using (var sReader = new StringReader(xml))
                using (var rdr = XmlReader.Create(sReader))
                {
                    Read(rdr, this);
                }
            }
            public void Read(XmlReader rdr)
            {
                Read(rdr, this);
            }
            private void Read(XmlReader rdr, JsonGenTree jsonGenTree)
            {
                while (rdr.Read())
                {
                    switch (rdr.NodeType)
                    {
                        case XmlNodeType.Element:
                            var node = new JsonGenTree(rdr.Name, jsonGenTree);
                            jsonGenTree.Children.Add(node);
                            Read(rdr, node);
                            break;
                        case XmlNodeType.Text:
                            jsonGenTree.Value = rdr.Value;
                            break;
                        case XmlNodeType.EndElement:
                            return;
                    }
                }
            }

        }

        public static string ToJson(this string xml)
        {
            return JsonGenTree.FromXml(xml).ToString();
        }
    }
}
