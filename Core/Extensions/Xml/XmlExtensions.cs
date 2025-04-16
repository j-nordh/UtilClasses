using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Extensions.Xml;

public static class XmlExtensions
{
    public static XmlNode FirstOrDefault(this XmlNodeList nodes, Func<XmlNode, bool> predicate)
    {
        return nodes?.Cast<XmlNode>().FirstOrDefault(predicate);
    }

    public static XmlNode Named(this XmlNodeList nodes, string name)
    {
        return nodes.FirstOrDefault(n => n.Name.EqualsIc2(name));
    }

    public static XmlAttribute FirstOrDefault(this XmlAttributeCollection attrs, Func<XmlAttribute, bool> predicate)
    {
        return attrs?.Cast<XmlAttribute>().FirstOrDefault(predicate);
    }

    public static XmlAttribute Named(this XmlAttributeCollection attrs, string name)
    {
        return attrs.FirstOrDefault(n => n.Name.EqualsIc2(name));
    }

    public static XmlNode ChildNamed(this XmlNode node, string name) => node?.ChildNodes.Named(name);
    public static XmlAttribute AttrNamed(this XmlNode node, string name) => node.Attributes.Named(name);

    public static bool ChangeValue(this XmlAttribute attr, string newVal, StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
    {
        if (attr.Value.EqualsIc2(newVal)) return false;
        attr.Value = newVal;
        return true;
    }

    public static XmlNode WithAttribute(this XmlNode n, string name, string value)
    {
        n.AddAttribute(name, value);
        return n;
    }

    public static XmlAttribute AddAttribute(this XmlNode n, string name, string value)
    {
        var attr = n.OwnerDocument.CreateAttribute(name);
        attr.Value = value;
        n.Attributes.Append(attr);
        return attr;
    }
    public static string GetValue(this XmlNode n, string xpath) => n.SelectSingleNode(xpath)?.Value ?? n.SelectSingleNode(xpath).InnerXml;
    public static IEnumerable<string> GetChildrensInnerText(this XmlNode node) => node.ChildNodes.Cast<XmlNode>().Select(n => n.InnerText);
    public static IEnumerable<string> GetChildrensInnerText(this XmlNode node, string xpath) => node.SelectNodes(xpath).Cast<XmlNode>().SelectMany(n=>n.GetChildrensInnerText());
}