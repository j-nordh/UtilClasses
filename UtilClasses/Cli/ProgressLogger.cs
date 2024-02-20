using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilClasses.Cli
{
    public class ProgressLogger<T>
    {
        private readonly Dictionary<int, string> _rows;
        private readonly Dictionary<int, int> _values;
        private readonly Func<T, int> _idExtractor;
        private readonly int _max;
        private readonly string _caption;
        private int _top;
        private int _left;
        private readonly int _barWidth;
        private readonly int _nameWidth;
        private static readonly char[] _symbols;

        static ProgressLogger()
        {
            _symbols = new[] {' ', '-', '+', '*', '#', '█'};
        }

        public ProgressLogger(Dictionary<int, string> rows, Func<T, int> idExtractor, int max, string caption)
        {
            _rows = rows;
            _idExtractor = idExtractor;
            _max = max;
            _caption = caption;
            _top = System.Console.CursorTop;
            _left = System.Console.CursorLeft;
            _barWidth = (int) Math.Ceiling((double) max/_symbols.Length);
            _barWidth += (max % _symbols.Length) > 0 ? 1 : 0;
            _nameWidth = rows.Select(kv => kv.Value.Length).Max();
            _values = rows.ToDictionary(kv => kv.Key, kv => 0);
            DrawTable();
        }

        private void DrawTable()
        {
            System.Console.WriteLine("┌" + Line(_nameWidth + 2) + "┬" + Line(_barWidth) + "┐");
            System.Console.WriteLine("│ {0,-" + _nameWidth + "} │" + Space(_barWidth) + "│", _caption);
            System.Console.WriteLine("├" + Line(_nameWidth + 2) + "┼" + Line(_barWidth) + "┤");
            foreach (var row in _rows)
            {
                var format = "│ {0,-" + _nameWidth + "} │" + Space(_barWidth) + "│";
                System.Console.WriteLine(format, row.Value);
            }
            System.Console.WriteLine("└" + Line(_nameWidth + 2) + "┴" + Line(_barWidth) + "┘");
        }

        public void Update(T val)
        {
            ++_values[_idExtractor(val)]; // = _values[_idExtractor(val)] + 1;
            DrawBars();
        }

        public void Update(IEnumerable<T> vals)
        {
            foreach (var val in vals)
            {
                ++_values[_idExtractor(val)];
            }
            DrawBars();
        }

        private void DrawBars()
        {
            int count = 0;
            foreach (var row in _rows)
            {
                DrawBar(count,row.Key);
                ++count;
            }
        }

        private void DrawBar(int bar, int id)
        {
            var orgTop = System.Console.CursorTop;
            var orgLeft = System.Console.CursorTop;
            System.Console.CursorTop = _top + 3 + bar;
            System.Console.CursorLeft = _left + _nameWidth + 4;
            int count = 0;
            int val = _values[id];
            if (val == _max)
            {
                System.Console.Write("Done");
                count = 4;
            }
            else
            {
                for (int i = 0; i < val / _symbols.Length; i++)
                {
                    System.Console.Write(_symbols.Last());
                    ++count;
                }
                System.Console.Write(_symbols[val % _symbols.Length]);
                ++count;
            }

            System.Console.Write(Space(_barWidth - count));
            System.Console.SetCursorPosition(orgLeft, orgTop);
        }

        private string c(char c, int count)
        {
            return count <= 0 ? "" : new string(c, count);
        }

        private string Space(int count)
        {
            return c(' ', count);
        }

        private string Line(int count)
        {
            return c('─', count);
        }
    }
}