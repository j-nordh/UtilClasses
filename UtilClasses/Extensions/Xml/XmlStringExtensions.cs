using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UtilClasses.Extensions.Xml
{
    public static class XmlStringExtensions
    {
        public static List<T> DeserializeXmlAsList<T>(this string xml, string rootName, bool throwOnUnknown=false)
        {
            List<T> ret;
            using (var rdr = new StringReader(xml))
            {
                var ser = new XmlSerializer(typeof(List<T>), new XmlRootAttribute(rootName));
                if (throwOnUnknown)
                {
                    ser.UnknownAttribute += UnknownAttribute;
                    ser.UnknownElement += UnknownElement;
                    ser.UnknownNode += UnknownNode;    
                }
                
                ret = (List<T>)ser.Deserialize(rdr);
            }
            return ret;
        }

        public static T DeserializeXml<T>(this string xml, bool throwOnUnknown = false)
        {
            T ret;
            using (var rdr = new StringReader(xml))
            {
                var ser = new XmlSerializer(typeof (T));
                if (throwOnUnknown)
                {
                    ser.UnknownAttribute += UnknownAttribute;
                    ser.UnknownElement += UnknownElement;
                    ser.UnknownNode += UnknownNode;  
                }
                ret = (T) ser.Deserialize(rdr);
            }
            return ret;
        }

        static void UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            throw new InvalidDataException("Unknown attribute: " + e.Attr.Name);
        }
        
        static void UnknownElement(object sender, XmlElementEventArgs e)
        {
            throw new InvalidDataException("Unknown element: " + e.Element.Name);
        }

        static void UnknownNode(object sender, XmlNodeEventArgs e)
        {
            throw new InvalidDataException("Unknown node: " + e.Name);
        }
    }
}
