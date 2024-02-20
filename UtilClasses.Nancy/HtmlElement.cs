using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Nancy
{
    public class HtmlElement
    {
        public List<HtmlElement> Children { get; }
        public string Text { get; set; }
        public string Tag { get; }
        public string Class { get { return Attributes.Maybe("class"); } set { Attributes["class"] = value; } }
        public Dictionary<string, string> Attributes;

        public HtmlElement(string tag)
        {
            Children = new List<HtmlElement>();
            Attributes = new Dictionary<string, string>();
            Tag = tag;
        }

        public HtmlElement(string tag, string text) :this(tag)
        {
            Text = text;
        }

        public HtmlElement(string tag, Dictionary<string, object> attributes):this(tag)
        {
            WithAttributes(attributes);
        }

        public HtmlElement(string tag, IEnumerable<HtmlElement> children): this(tag)
        {
            Children.AddRange(children);
        }

        public HtmlElement WithChild(string tag, string text) => Do(()=>AddChild(tag).WithText(text));

        public HtmlElement WithChild(string tag, string cls, string text) =>
            Do(() => AddChild(tag).WithClass(cls).WithText(text));

        public HtmlElement WithDiv(string cls, string text) => WithChild("div", cls, text);

        public HtmlElement WithDiv(string cls, Action<HtmlElement> a)
        {
            var child = AddChild("div").WithClass(cls);
            a(child);
            return this;
        }
        public HtmlElement AddChild(string tag) => AddChild(new HtmlElement(tag));

        public HtmlElement AddChild(HtmlElement e)
        {
            Children.Add(e);
            return e;
        }

        public HtmlElement AddChild(string tag, IEnumerable<HtmlElement> children)
        {
            var e = AddChild(tag);
            e.Children.AddRange(children);
            return e;
        }

        public HtmlElement AddDiv(string cls) => AddChild("div").WithClass(cls);

        public enum TagStyle
        {
            Start,
            End,
            Single
        }
        public string ToString(TagStyle ts)
        {
            var attr = Attributes.Select(kvp => $"{kvp.Key}=\"{kvp.Value}\"").Join(" ");
            attr = attr.IsNullOrWhitespace() ? attr : " " + attr;
            if (ts == TagStyle.Single && attr.IsNullOrEmpty() && !Tag.EqualsIc2("br") && !Tag.EqualsIc2("td") && !Tag.EqualsIc2("hr"))
                return "";
            var start = $"<{Tag}{attr}{(ts==TagStyle.Single?"/":"")}>";
            switch (ts)
            {
                case TagStyle.Start:
                case TagStyle.Single:
                    return start;
                case TagStyle.End:
                    return $"</{Tag}>";
                   
                default:
                    throw new ArgumentOutOfRangeException(nameof(ts), ts, null);
            }
        }

        public HtmlElement WithClass(string c) => Do(() => Attributes["class"] = c);

        public HtmlElement WithAttribute(string key, object val) => Do(()=>Attributes[key] = val.ToString());

        public HtmlElement WithAttributes(Dictionary<string, object> attrs) => Do(() =>
            Attributes = Attributes.UnionDict(attrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString())));

        public HtmlElement WithAttributes(string type=null, string onClick =null, string cssClass = null, int? size =null, string name=null, object value=null, string id=null)
        {
            var dict = new Dictionary<string, string>
            {
                {"type", type},
                {"onclick", onClick},
                {"class", cssClass},
                {"size", size?.ToString()},
                {"name", name},
                {"value", value?.ToString()},
                {"id", id}
            };
            foreach (var kvp in dict.Where(i=>i.Value!= null))
            {
                Attributes[kvp.Key]= kvp.Value;
            }
            return this;
        }

        public HtmlElement MaybeWithAttribute(string key, string value)
        {
            if (value.IsNotNullOrEmpty())
                WithAttribute(key, value);
            return this;
        }
        public HtmlElement WithAttributes(IEnumerable<KeyValuePair<string, object>> attrs) =>
            WithAttributes(attrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        public HtmlElement WithChild(HtmlElement child) => Do(()=>Children.Add(child));
        public HtmlElement WithChild(string tag) => WithChild(new HtmlElement(tag));

        public HtmlElement WithChild(string tag, Dictionary<string, object> attributes) =>
            Do(() => AddChild(tag).WithAttributes(attributes));

        public HtmlElement WithChild(string tag, Action<HtmlElement> action) => Do(() => action(AddChild(tag)));
        public HtmlElement WithChildren(IEnumerable<HtmlElement> children) => Do(() => Children.AddRange(children));
        public HtmlElement ModifyChildren(Action<List<HtmlElement>> modder) => Do(() => modder(Children));
        public HtmlElement OnFirstChild(Action<HtmlElement> modder) => Do(() => modder(Children.First()));
        public HtmlElement OnFirstRow(Action<HtmlElement> modder) => Do(() => modder(Children.First()));

        public HtmlElement OnChild(int index, Action<HtmlElement> modder) =>
            Do(() => modder(Children.Skip(index).First()));
        private HtmlElement Do(Action a)
        {
            a();
            return this;
        }

        public static HtmlElement CaptionedListColumn(string caption, IEnumerable<HtmlElement> items) =>
            CaptionedColumn(caption).WithChild(new HtmlElement("ul", items.Select(i=>new HtmlElement("li").WithChild(i))));

        public static HtmlElement CaptionedColumn(string caption) =>
            new HtmlElement("div").WithClass("column").WithChild(Caption(caption));

        public static HtmlElement CaptionedColumn(string caption, params HtmlElement[] content) =>
            CaptionedColumn(caption).WithChildren(content);
        public static HtmlElement Caption(string caption) => new HtmlElement("div", caption).WithClass("caption");
        public static HtmlElement SubCaption(string caption) => new HtmlElement("div", caption).WithClass("subCaption");
        public static HtmlElement UnorderedList(IEnumerable<HtmlElement> items) => UnorderedList().AddMultiple("li", items);
        public static HtmlElement UnorderedList(IEnumerable<string> items) => UnorderedList().AddMultiple("li", items);
        public static HtmlElement UnorderedList() => new HtmlElement("ul");
        public static HtmlElement Link(string text, string url) => new HtmlElement("a", text).WithAttribute("href", url);
        public static HtmlElement TableRow()=>new HtmlElement("tr");
        public static HtmlElement Table()=>new HtmlElement("table");
        public HtmlElement AddTr() => AddChild("tr");
        public HtmlElement AddTd(int? colSpan = null, int? rowSpan =null)
        {
            var ret = AddChild("td");
            if (colSpan != null) ret.WithAttribute("colspan", colSpan.Value);
            if (rowSpan != null) ret.WithAttribute("rowspan", rowSpan.Value);
            return ret;
        }

        public int? RowSpan
        {
            get { return Attributes.Maybe("rowspan")?.AsInt(); }
            set { Attributes["rowspan"] = value?.ToString(); }
        }

        public int? ColSpan
        {
            get { return Attributes.Maybe("colspan")?.AsInt(); }
            set { Attributes["colspan"] = value?.ToString(); }
        }

        public HtmlElement WithRowSpan(int? i) => Do(() => RowSpan = i);
        public HtmlElement WithColSpan(int? i) => Do(() => ColSpan = i);
        public HtmlElement AddTd(string txt, int? colSpan = null, int? rowSpan = null) => AddTd(colSpan, rowSpan).WithText(txt);
        public HtmlElement AddTd(HtmlElement child, int? colSpan = null, int? rowSpan = null) => AddTd(colSpan, rowSpan).WithChild(child);
        public HtmlElement AddTd(IEnumerable<HtmlElement> children, int? colSpan = null, int? rowSpan = null) => AddTd(colSpan, rowSpan).WithChildren(children);
        public HtmlElement AddTh(int? colSpan = null, int? rowSpan = null)
        {
            var ret = AddChild("th");
            if (colSpan != null) ret.WithAttribute("colspan", colSpan.Value);
            if (rowSpan != null) ret.WithAttribute("rowspan", rowSpan.Value);
            return ret;
        }

        public HtmlElement AddTh(string txt, int? colSpan = null, int? rowSpan = null) => AddTh(colSpan, rowSpan).WithText(txt);
        public HtmlElement AddLi() => AddChild("li");
        public HtmlElement AddLi(string txt) => AddLi().WithText(txt);
        public HtmlElement AddLi(HtmlElement child) => AddLi().WithChild(child);

        public HtmlElement WithTd(string txt, int? colSpan = null, int? rowSpan = null) => Do(() => AddTd(colSpan, rowSpan).WithText(txt));

        public HtmlElement WithTd(Action<HtmlElement> tdAction, int? colSpan = null, int? rowSpan = null) =>
            Do(() => tdAction(AddTd(colSpan, rowSpan)));
        public HtmlElement WithTd(HtmlElement child, int? colSpan = null, int? rowSpan = null) => Do(() => AddTd(colSpan, rowSpan).WithChild(child));
        public HtmlElement WithTd(IEnumerable<HtmlElement> children, int? colSpan = null, int? rowSpan = null) => Do(() => AddTd(colSpan, rowSpan).WithChildren(children));
        public HtmlElement WithTh(string txt, int? colSpan = null, int? rowSpan = null) => Do(()=>AddTh(colSpan, rowSpan).WithText(txt));
        public HtmlElement WithTh(int? colSpan = null, int? rowSpan = null) => Do(() => AddTh(colSpan, rowSpan));
        public HtmlElement WithText(string txt) => Do(() => Text = txt);
        public HtmlElement WithLi(string txt) => Do(() => AddLi().WithText(txt));

        public HtmlElement WithTr(Action<HtmlElement> a) => Do(() =>
        {
            var tr = AddTr();
            a(tr);
        });

        
        public HtmlElement AddTds(IEnumerable<object> cellTexts) => AddMultiple("td", cellTexts);
        public HtmlElement AddThs(IEnumerable<object> cellTexts) => AddMultiple("th", cellTexts);

        public HtmlElement AddMultiple(string tag, IEnumerable<HtmlElement> elements) =>
            Do(() => elements.ForEach(e => AddChild(tag).WithChild(e)));
        public HtmlElement AddMultiple(string tag, IEnumerable<object> cellTexts)
        {
            cellTexts.ForEach(t => AddChild(tag).Text = t.ToString());
            return this;
        }

        public static HtmlElement Div(string text="")
        {
            return new HtmlElement("div", text);
        }

        public HtmlElement AppendClass(string s)
        {
            if (s.IsNullOrWhitespace()) return this;
            Class = Class + (Class.IsNullOrEmpty() ? "" : " ") + s;
            return this;
        }

        public HtmlElement WithWidth(string width) => WithAttribute("width", width);
        public HtmlElement WithAlign(string align) => WithAttribute("align", align);
    }
}