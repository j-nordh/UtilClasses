using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Lists;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Cli
{
    public class TextFramer
    {
        private readonly BorderHandler _bh;
        public List<List<string>> Matrix { get; set; } = new();

        public string HorizontalPadding { get; set; } = "";

        public TextFramer(BorderHandler bh)
        {
            _bh = bh;
        }

        public TextFramer AddRow(List<string> row)
        {
            Matrix ??= new();
            Matrix.Add(row);
            return this;
        }

        public TextFramer AddRow(params string[] row) => AddRow(row.ToList());

        public string Draw()
        {
            var exploded = ExplodeMatrix(out var widths);
            var sb = new IndentingStringBuilder("  ");
            sb.AppendLine(_bh.Top.Draw(widths));

            for (var r = 0; r < exploded.Count; r++)
            {
                var row = exploded[r];
                for (int i = 0; i < row[0].Count; i++)
                {
                    var content = row.Select(c => c[i]).ToList();
                    sb.AppendLine(_bh.Content.Draw(widths, content));
                }

                if (r < exploded.Count - 1)
                    sb.AppendLine(_bh.Separator.Draw(widths));
            }

            sb.AppendLine(_bh.Bottom.Draw(widths));
            return sb.ToString();
        }

        private List<List<List<string>>> ExplodeMatrix(out List<int> widths)
        {
            var ret = new List<List<List<string>>>();
            var heights = Matrix.Select(row => row.Select(c => c.LineCount()).Max()).ToArray();
            var columns = Matrix.Max(row => row.Count);
            widths = new List<int>(Enumerable.Repeat(0, columns));
            for (int y = 0; y < heights.Length; y++)
            for (var x = 0; x < columns; x++)
            {
                var lines = (Matrix[y].Maybe(x) ?? "").SplitLines();
                for (int l = 0; l < heights[y]; l++)
                {
                    var line = l < lines.Length ? lines[l] : "";
                    line = $"{HorizontalPadding}{line}{HorizontalPadding}";
                    ret.GetOrAdd(y).GetOrAdd(x).SetItem(l, line);
                    if (line.Length > widths[x])
                        widths[x] = line.Length;

                }
            }
            return ret;
        }
    }
}
