using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Cli;
using UtilClasses.Interfaces;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;

namespace UtilClasses
{
    public partial class TextTable
    {
        protected readonly List<int> _widths;
        protected readonly Row _headings;
        protected readonly List<Row?> _rows;
        public BorderHandler Borders { get; set; } = new();

        protected TableWriter _writer;
        public bool ShowHeader { get; set; }
        public int SpacerInterval { get; set; }
        private class IndentingTableWriter : TableWriter
        {

            public IndentingTableWriter(IndentingStringBuilder sb) : base(sb)
            {
            }
            public override void Begin() { }
            public override void End() { }
            public override void Write(string s) => Sb.Append(s);
            public override void Write(char c) => Sb.Append(c);
            public override void WriteLine(char c) => Sb.AppendLine(c);
        }

        public TextTable(IndentingStringBuilder sb, params string[] headings) : this(new IndentingTableWriter(sb), headings.ToList())
        { }

        public TextTable(IndentingStringBuilder sb, IEnumerable<string> headings) : this(new IndentingTableWriter(sb), headings)
        { }
        public TextTable(params string[] headings) : this(new IndentingTableWriter(new IndentingStringBuilder("  ")), headings.ToList())
        { }

        public TextTable(IEnumerable<string> headings) : this(new IndentingTableWriter(new IndentingStringBuilder("  ")), headings)
        { }

        public static TextTable HiddenTrimmed(params string[] headings) => new(headings)
        {
            Borders = BorderHandler.None(),
            SpacerInterval = -1,
            ShowHeader = false,
            TrimOutput = true
        };
        public TextTable(TableWriter writer, IEnumerable<string> headings)
        {
            _headings = new Row(headings, Borders);
            _widths = _headings.Values.Select(s => s?.Length??0).ToList();
            _rows = new();
            _writer = writer;
            ShowHeader = true;
            SpacerInterval = 5;
        }

        public TextTable AddRow(params object[] row) => AddRow(new Row(row.Select(o => o?.ToString()), Borders));
        public TextTable AddRow(IEnumerable<string>? row) => AddRow(null== row?null: new Row(row.ToList(), Borders));
        public TextTable AddRow(IEnumerable<string> values, Action<Row> config)
        {
            var row = new Row(values.ToList(), Borders);
            AddRow(row);
            config?.Invoke(row);
            return this;
        }
        public TextTable AddRow(TextTable.Row? r)
        {
            _rows.Add(r);
            if (null == r) return this;
            //update max width
            for (int i = 0; i < _headings.Values.Count && i < r.Values.Count; i++)
            {
                foreach (var l in r.Values[i]?.SplitLines()??new string?[]{})
                {
                    if(null == l) continue;
                    if (l.Length > _widths[i])
                        _widths[i] = l.Length;
                }
            }
            return this;
        }

        public TextTable AddRow(IHasTableRow o) => AddRow(o.GetTableRow());
        public TextTable AddRows(IEnumerable<IHasTableRow> os)
        {
            os.ForEach(AddRow);
            return this;
        }
        public TextTable AddRows(IEnumerable<IEnumerable<string>> rows)
        {
            rows.ForEach(AddRow);
            return this;
        }
        public TextTable AddRows(int count, params string[] os)
        {
            for (int i = 0; i < count; i++)
                AddRow(os.AsEnumerable());
            return this;
        }

        public IndentingStringBuilder Sb => _writer.Sb;

        protected void Pad(string val, char padding, int width)
        {
            _writer.Write(val);
            _writer.Write(new string(padding, width - val.Length));
        }

        public string Draw()
        {

            DrawRow(Borders.Top);
            if (ShowHeader) _headings.Render(_widths, _writer, Borders.Content);
            for (int i = 0; i < _rows.Count; i++)
            {
                if (SpacerInterval > 0 && i % SpacerInterval == 0 && i < _rows.Count - 2) DrawRow(Borders.Separator);
                if (null == _rows[i]) { Row.RenderEmpty(_widths, _writer, Borders.Separator); continue; }
                _rows[i]?.Render(_widths, _writer, Borders);
            }

            DrawRow(Borders.Bottom);
            var ret = Sb.ToString();
            if (TrimOutput)
                ret = ret.Trim().SplitLines().Trim().Join("\n");
            return ret;
        }
        private void DrawRow(BorderRow r) => Row.RenderEmpty(_widths, _writer, r);

        public void Spacer() => _rows.Add(null);

        public static TextTable Merge(params (string header, string text)[] columns)
        {
            var cols = columns.Select(c => c.text.SplitLines()).ToList();
            var max = cols.Max(c => c.Length);
            var tbl = new TextTable(columns.Select(c=>c.header).ToArray())
            {
                SpacerInterval = -1,
                ShowHeader = true
            };
            tbl.Spacer();
            for (int i = 0; i < max; i++)
            {
                var row = new List<string>();
                foreach (var col in cols)
                {
                    row.Add(col.Length>i?col[i]:"");
                }
                tbl.AddRow(row);
            }

            return tbl;
        }
        public bool TrimOutput { get; set; }

        public TextTable WithTrimmedOutput(bool trim = true)
        {
            TrimOutput = true;
            return this;
        }
    }

    public class BorderHandler : ICloneable<BorderHandler>
    {
        private bool _lineOnNull;
        private BorderRow[] _allRows;

        public static class Patterns
        {
            public const string DefaultPattern =        "┌─┬┐\n" +
                                                        "│ ││\n" +
                                                        "├─┼┤\n" +
                                                        "└─┴┘";

            public const string FramePattern =          "┌──┐\n" +
                                                        "│  │\n" +
                                                        "│  │\n" +
                                                        "└──┘";
            public const string NonePattern =           "    \n" +
                                                        "    \n" +
                                                        "    \n" +
                                                        "    ";
            public const string DoublePattern =         "╔═╦╗\n" +
                                                        "║ ║║\n" +
                                                        "╠═╬╣\n" +
                                                        "╚═╩╝";
            public const string DoubleFramePattern =    "╔═╤╗\n" +
                                                        "║ │║\n" +
                                                        "╟─┼╢\n" +
                                                        "╚═╧╝";
        }

        public static BorderHandler Default() => new(Patterns.DefaultPattern);
        public static BorderHandler Frame() => new(Patterns.FramePattern);
        public static BorderHandler None() => new(Patterns.NonePattern);
        public static BorderHandler Double() => new(Patterns.DoublePattern);
        public static BorderHandler DoubleFrame() => new(Patterns.DoubleFramePattern);
        public BorderRow Top { get; } = new();
        public BorderRow Content { get; } = new();
        public BorderRow Separator { get; } = new();
        public BorderRow Bottom { get; } = new();

        public BorderHandler(BorderHandler o)
        {
            Top = o.Top.Clone();
            Content = o.Content.Clone();
            Separator = o.Separator.Clone();
            Bottom = o.Bottom.Clone();
            _lineOnNull = o.LineOnNull;
            _allRows = new[] { Top, Content, Separator, Bottom };

        }
        public BorderHandler() : this(Patterns.DefaultPattern) { }
        public BorderHandler(string pattern)
        {
            var lines = pattern.SplitLines();
            if (lines.Length != 4)
                throw new ArgumentException(
                    "The provided pattern must be a string with 4 rows, each with 4 characters");
            _allRows = new[] { Top, Content, Separator, Bottom };
            for (var row = 0; row < lines.Length; row++)
            {
                var line = lines[row];
                if (line.Length != 4)
                    throw new ArgumentException(
                        "The provided pattern must be a string with 4 rows, each with 4 characters");
                _allRows[row].Parse(line);
            }
        }

        public bool LineOnNull
        {
            get => _lineOnNull;
            set
            {
                _lineOnNull = value;
                _allRows.ForEach(r => r.LineOnNull = value);
            }
        }
        public char Between(string prev, string val)
        {
            return (prev, val) switch
            {
                (_, _) when !LineOnNull => Content.Between,
                (null, null) => Separator.Between,
                (null, _) => Separator.Right,
                (_, null) => Separator.Left,
                _ => Content.Between
            };
        }

        public char Left(string? val) => GetRow(val).Left;
        public char Padding(string? val) => GetRow(val).Padding;
        public char Right(string? val) => GetRow(val).Right;
        public BorderRow GetRow(string? val) => null == val && LineOnNull ? Separator : Content;
        public BorderHandler Clone() => new(this);
    }

    public class BorderRow : ICloneable<BorderRow>
    {
        public char Left { get; set; }
        public char Padding { get; set; }
        public char Between { get; set; }
        public char Right { get; set; }
        public bool LineOnNull { get; set; }

        public BorderRow()
        {

        }

        public BorderRow(BorderRow o)
        {
            Left = o.Left;
            Padding = o.Padding;
            Between = o.Between;
            Right = o.Right;
            LineOnNull = o.LineOnNull;
        }

        public void Parse(string s)
        {
            if (s.Length != 4)
                throw new ArgumentException(
                    "The provided pattern must be a string with 4 rows, each with 4 characters");
            Left = s[0];
            Padding = s[1];
            Between = s[2];
            Right = s[3];
        }

        public string Draw(IEnumerable<int> widths)
        {
            var sb = new IndentingStringBuilder("")
                .Append(Left);
            foreach (var width in widths)
            {
                sb.Append(Padding, width).Append(Between);
            }

            sb.Backspace().Append(Right);
            return sb.ToString();
        }

        public string Draw(List<int> widths, List<string> content, int horizontalPadding =0)
        {
            var sb = new IndentingStringBuilder("").Append(Left);
            for (int i = 0; i < widths.Count; i++)
            {
                var txt = content.Count > i ? content[i] : "";
                txt = txt.PadRight(widths[i], ' ');
                sb.Append(txt)
                    .Append(Between);
            }

            sb.Backspace().Append(Right);
            return sb.ToString();
        }
        public BorderRow Clone() => new(this);
    }
}