using System.Collections.Generic;
using System.Xml;

namespace UtilClasses.Extensions.Xml.ValueReader
{

    public class XmlTextValueReader : XmlValueReader
    {
        public XmlTextValueReader(string key) : this(key, null, null)
        {

        }

        public XmlTextValueReader(string key, string parent) : this(key, parent, null)
        {
        }

        public XmlTextValueReader(string key, IEnumerable<XmlValueReader> subReaders) : this(key, "", subReaders)
        {
        }

        public XmlTextValueReader(string key, string? parent, IEnumerable<XmlValueReader>? subReaders)
            : base(key, parent ?? "", subReaders ?? new List<XmlValueReader>())
        {
        }

        public XmlTextValueReader(string key, XmlValueReader subReader)
            : this(key, "", new List<XmlValueReader> { subReader })
        {
        }


        protected override void _Update(XmlReader rdr)
        {
            if (!_inTag) return;
            if (_subReaders.Count > 0) return;
            if (rdr.NodeType != XmlNodeType.Text) return;
            if (string.IsNullOrWhiteSpace(rdr.Value)) return;
            _val = rdr.Value;
            RaiseStringFound();
        }
    }
}

