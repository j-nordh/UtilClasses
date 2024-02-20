using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Cli;

namespace UtilClasses
{

    public partial class TextTable
    {
        public class Row
        {
            public Row(BorderHandler borders)
            {
                Borders = borders.Clone();
            }
            public Row(IEnumerable<string?>? vals, BorderHandler borders) : this(borders)
            {
                Values = vals?.ToList() ?? new List<string?>();
            }
            public List<string?> Values { get; set; }
            public bool LineOnNull
            {
                get => Borders.LineOnNull;
                set => Borders.LineOnNull = value;
            }
            private void CombinedRow(List<string> values)
            {

            }

            public BorderHandler Borders { get; set; }

            public static void Render(List<string?> values, List<int> widths, TableWriter writer, BorderRow br) =>
                Render(values, widths, writer, _ => br.Left, _ => br.Padding, (_, _) => br.Between, _ => br.Right);

            public void Render(List<int> widths, TableWriter writer, BorderRow br) => Render(Values, widths, writer, br);
            public void Render(List<int> widths, TableWriter writer, BorderHandler bh) =>
                Render(Values, widths, writer, bh.Left, bh.Padding, bh.Between, bh.Right);
            public static void RenderEmpty(List<int> widths, TableWriter writer, BorderRow br) =>
                Render(new(), widths, writer, _ => br.Left, _ => br.Padding, (_, _) => br.Between, _ => br.Right);

            public static void Render(List<string?> values, List<int> widths, TableWriter writer, BorderHandler bh) =>
                Render(values, widths, writer, bh.Left, bh.Padding, bh.Between, bh.Right);
            private static void Render(List<string?> values, List<int> widths, TableWriter writer, Func<string?, char> left, Func<string?, char> padding, Func<string?, string?, char> between, Func<string?, char> right)
            {

                string? val = null;
                string? prev = null;
                char Get(Func<string?, string?, char> f) => f(prev, val);
                bool first = true;
                for (int i = 0; i < widths.Count(); i++)
                {
                    val = null;
                    if (null != values && values.Count > i)
                        val = values[i];
                    if (first)
                    {
                        writer.Write(left(val));
                        first = false;
                    }
                    else
                    {
                        writer.Write(Get(between));
                    }
                    int width = widths[i];
                    Pad(writer, val, padding(val), width);
                    prev = val;
                }

                writer.WriteLine(right(val));
            }
            private static void Pad(TableWriter writer, string? val, char padding, int width)
            {
                val ??= "";
                writer.Write(val);
                writer.Write(new string(padding, width - val.Length));
            }
        }
    }
}