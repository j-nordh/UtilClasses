using System.Collections.Generic;
using System.Xml;

namespace UtilClasses.Core.Extensions.Xml.ValueReader;

public class XmlAttributeReader : XmlValueReader
{
    private readonly string _attribute;
    public XmlAttributeReader(string key, string attribute) : this(null, key, attribute)
    { }
    public XmlAttributeReader(string? parent, string key, string attribute) : base(key, parent??"", null?? new List<XmlValueReader>())
    {
        _attribute = attribute;
    }

    protected override void _Update(XmlReader rdr)
    {
        if (!_inTag) return;
        _val = rdr.GetAttribute(_attribute);
        RaiseStringFound();
    }
}