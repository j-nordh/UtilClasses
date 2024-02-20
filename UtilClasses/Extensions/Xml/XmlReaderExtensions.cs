using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UtilClasses.Extensions.Xml
{
    public static class XmlReaderExtensions
    {
        public static bool IsEndElement(this XmlReader value, string name)
        {
            return value.NodeType == XmlNodeType.EndElement && value.Name == name;
        }
        public static bool IsEndElement(this XmlReader value)
        {
            return value.NodeType == XmlNodeType.EndElement;
        }

        public static bool IsAttribute(this XmlReader value, string name)
        {
            return value.NodeType == XmlNodeType.Attribute && value.Name == name;
        }
    }
}
