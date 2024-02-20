using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Nancy
{
    public static class HtmlElementExtensions
    {
        public static IEnumerable<HtmlElement> ToLinks(this IEnumerable<KeyValuePair<string, string>> items) =>
            items.Select(kvp => HtmlElement.Link(kvp.Key, kvp.Value));

        public static HtmlElement ToTable(this IEnumerable<KeyValuePair<string, object>> items, bool vertical = true,
            bool useTh = true)
        {
            var tbl = new HtmlElement("table");
            if (vertical)
            {
                foreach (var i in items)
                {
                    var row = tbl.AddTr();
                    (useTh ? row.AddChild("th") : row.AddTd()).Text = i.Key;
                    row.AddTd().Text = i.Value?.ToString()??"";
                }
            }
            else
            {
                var top = tbl.AddTr();
                var bottom = tbl.AddTr();
                foreach (var i in items)
                {
                    (useTh ? top.AddChild("th") : top.AddTd()).Text = i.Key;
                    bottom.AddTd().Text = i.Value.ToString();
                }
            }
            return tbl;
        }

        public static HtmlElement ToTableHeaderRow(this IEnumerable<string> headers) => new HtmlElement("tr").AddThs(headers);
        public static HtmlElement ToTableRow(this IEnumerable<string> vals) => new HtmlElement("tr").AddTds(vals);
        public static IndentingStringBuilder Append(this IndentingStringBuilder strb, HtmlElement e)
        {
            if (null == e) return strb;
            if (e.Text.IsNullOrEmpty() && !e.Children.Any()) return strb.Append(e.ToString(HtmlElement.TagStyle.Single));
            return strb
                .Append($"{e.ToString(HtmlElement.TagStyle.Start)}{e.Text}")
                .Indent(e.Children.Any(), sb => e.Children.ForEach(c => sb.Append(c)))
                .AppendLine($"{e.ToString(HtmlElement.TagStyle.End)}");
        }

        public static IndentingStringBuilder Append(this IndentingStringBuilder strb, IEnumerable<HtmlElement> es)
        {
            es.ForEach(e => strb.Append(e));
            return strb;
        }
        public static List<HtmlElement> ToTableRows<T>(this IEnumerable<Expression> expressions, T obj,
            bool vertical = true)
        {

            var props = typeof(T).GetProperties().ToDictionary(p => p.Name, p => p.GetValue(obj)?.ToString());
            var fields = typeof(T).GetFields().ToDictionary(f => f.Name, f => f.GetValue(obj)?.ToString());
            var kvs = expressions.Select(exp =>
            {
                var lambda = exp as LambdaExpression;
                if (lambda == null)
                    throw new ArgumentNullException(nameof(expressions), @"That's not even a lambda expression!");

                MemberExpression me = null;

                switch (lambda.Body.NodeType)
                {
                    case ExpressionType.Convert:
                        me = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                        break;
                    case ExpressionType.MemberAccess:
                        me = lambda.Body as MemberExpression;
                        break;
                }
                if (me == null) throw new ArgumentException("Expressions must be on the form ()=>object.Property");
                var name = me.Member.Name;
                return new KeyValuePair<string, string>(name, props.Maybe(name) ?? fields.Maybe(name));
            });
            return kvs.ToTableRows(vertical);
        }

        public static List<HtmlElement>
            ToTableRows(this IEnumerable<KeyValuePair<string, string>> kvs, bool vertical) => kvs
            .Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value)).ToTableRows(vertical);
        public static List<HtmlElement> ToTableRows(this IEnumerable<KeyValuePair<string, object>> kvs, bool vertical)
        {
            if (vertical)
            {
                return kvs.Select(kv => HtmlElement.TableRow().WithTh(kv.Key).WithTd(kv.Value)).ToList();
            }
            var headerRow = HtmlElement.TableRow();
            var valueRow = HtmlElement.TableRow();
            foreach (var kv in kvs)
            {
                headerRow.AddTh(kv.Key);
                valueRow.AddTd(kv.Value?.ToString() ?? "");
            }
            return new[] { headerRow, valueRow }.ToList();
        }

        private static HtmlElement WithTd(this HtmlElement tr, object o)
        {
            var elem = o as HtmlElement;
            return null != elem ? tr.WithTd(elem) : tr.WithTd(o?.ToString());
        }
        public static HtmlElement ToTable<T>(this IEnumerable<Expression> expressions, T obj,
            bool vertical = true) => new HtmlElement("table").WithChildren(expressions.ToTableRows(obj, vertical));

        public static HtmlElement WithRows<T>(this HtmlElement e, T obj, bool vertical, params Expression<Func<object>>[] expressions)
        {
            return e.WithChildren(expressions.ToTableRows(obj, vertical)).AppendClass(vertical?"vertical":"");
        }

        public static HtmlElement WithLabelledRow(this HtmlElement e, string label, Action<HtmlElement> rowAction)
        {
            var row = e.AddTr().WithTd(td => td.AddChild("div").WithClass("rotated subCaption").WithText(label));
            rowAction(row);
            return e;
        }

        public static HtmlElement WithLabelledRow(this HtmlElement e, string label, HtmlElement child) =>
            e.WithLabelledRow(label, r => r.AddTd(child));
        public static HtmlElement WithLabelledRow(this HtmlElement e, string label, IEnumerable<HtmlElement> children) =>
            e.WithLabelledRow(label, r => r.AddTd(children));

        public static HtmlElement WithHrRow(this HtmlElement e, int colspan, int widthPercent) => e.WithTr(tr =>
            tr.WithTd(new HtmlElement("hr").WithAttribute("width", $"{widthPercent}%"), colspan));
        public static HtmlElement ToUnorderedList<T>(this IEnumerable<T> items, Func<T, string> captionFunc) =>
            HtmlElement.UnorderedList(items.Select(captionFunc));

        public static HtmlElement ToUnorderedList(this IEnumerable<HtmlElement> items)=>HtmlElement.UnorderedList(items);

        public static HtmlElement ToTable<T>(this IEnumerable<T> items, bool vertical, Func<T, string> captionFunc,
            Func<T, object> valueFunc)
            => new HtmlElement("table")
                .WithChildren(items
                    .Select(i => new KeyValuePair<string, object>(captionFunc(i), valueFunc(i)))
                    .ToTableRows(vertical));

        public static List<HtmlElement> ToTableRows<T>(this IEnumerable<T> items, bool vertical,
            Func<T, string> captionFunc, params Func<T, object>[] valueFuncs)
        {
            var ret = new List<HtmlElement>();
            if (vertical)
            {
                foreach (var item in items)
                {
                    var tr = HtmlElement.TableRow();
                    tr.AddTh(captionFunc(item));
                    foreach (var f in valueFuncs)
                    {
                        tr.WithTd(f(item));
                    }
                    ret.Add(tr);
                }
                return ret;
            }
            for (int i = 0; i <= valueFuncs.Length; i++) ret[i] = HtmlElement.TableRow();
            foreach (var item in items)
            {
                ret[0].WithTh(captionFunc(item));
                for (int i = 0; i < valueFuncs.Length; i++)
                {
                    ret[i + 1].WithTd(valueFuncs[i](item));
                }
            }
            return ret;
        }

        

        public static int Rows(this HtmlElement table)
        {
            if (!table.Tag.EqualsIc2("table")) throw new ArgumentException("Row calculation only works for tables...");
            return table.Children.Count(c => c.Tag.EqualsIc2("tr"));
        }

        public static int MaxColumns(this HtmlElement t)
        {
            if (t.Tag.EqualsIc2("table"))
                return t.Children.Where(c => c.Tag.EqualsIc2("tr")).Select(c => c.MaxColumns()).Max();
            if (t.Tag.EqualsIc2("tr"))
                return t.Children.Count(c => c.Tag.EqualsIc2("td") || c.Tag.EqualsIc2("th"));
            throw new ArgumentException("Row calculation only works for tables and rows...");
        }
    }
}
