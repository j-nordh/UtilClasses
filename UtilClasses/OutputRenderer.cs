using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;
using System.Linq;
using UtilClasses.Cli;

namespace UtilClasses
{
    public static class OutputRenderer
    {
        public class Item
        {
            public string Caption { get; set; }
            public IEnumerable<object> Values { get; set; }
            public int Indent { get; set; }
            public bool IsSeparator { get; set; }
            public Item() { }

            public Item(bool isSeparator, int indent, string caption, params object[] vals)
            {
                IsSeparator = isSeparator;
                Indent = indent;
                Caption = caption;
                Values = vals;
            }
            public Item(bool isSeparator, string caption, params object[] vals) :this(isSeparator, 0, caption, vals)
            { }

            public Item(string caption, params object[] vals) : this(false, 0, caption, vals) { }
        }
        public static void RenderCsv(string path, IEnumerable<Item> items)
        {
            var sb = new IndentingStringBuilder("");
            foreach (var i in items)
            {
                if (null == i) continue;
                if (i.IsSeparator)
                {
                    sb.AppendLine();
                }
                var cap = i.Caption.IsNotNullOrEmpty() ? $"{ i.Caption };" : "";
                sb.AppendLines($"{cap}{i.Values.Select(v => v.ToString()).Join(";")}");
            }
            File.WriteAllText(Path.Combine(path, "Metadata.csv"), sb.ToString());
        }

        public static void RenderText(string path, IEnumerable<Item> items)
        {
            var sb = new IndentingStringBuilder("\t");
            var section = 0;
            TextTable? tbl = null;
            foreach (var i in items)
            {
                if (i.IsSeparator)
                {
                    section += 1;
                }
                switch (section)
                {
                    case 0:
                        sb.Indent(i.Indent,
                            () => sb.AppendLines($"{i.Caption}: {i.Values.Select(v => v.ToString()).Join(" ")}"));
                        break;
                    case 1 when null == tbl:
                        tbl = new TextTable(sb, i.Values.Select(v => v.ToString()));
                        section += 1;
                        continue;
                    case 2:
                        tbl?.AddRow(new[] { i.Caption }.Union(i.Values.Select(v => v.ToString())));
                        break;
                }
            }
            tbl?.Draw();
            File.WriteAllText(Path.Combine(path, "Metadata.txt"), sb.ToString());
        }
        public static void Add(this List<Item> lst, string caption, params object[] values) =>
            lst.Add(new Item { Caption = caption, Values = values });
        public static void Add(this List<Item> lst, string caption, params string[] values) =>
            lst.Add(new Item { Caption = caption, Values = values });
        public static void Add(this List<Item> lst, string caption, int indent, params object[] values) =>
            lst.Add(new Item { Caption = caption, Values = values, Indent = indent });
        public static void Add(this List<Item> lst, bool predicate, string caption, int indent, params object[] values)
        {
            if (!predicate) return;
            lst.Add(new Item { Caption = caption, Values = values, Indent = indent });
        }

        public static void Add(this List<Item> lst, bool _, params string[] captions) =>
            lst.Add(new Item { IsSeparator = true, Values = captions });

        public static void Add(this List<Item> lst, IHasTableRow r) => lst.Add(new Item()
            { Caption = r.GetTableRow().First(), Values = r.GetTableRow().Skip(1) });
    }
}
