using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Cli;

public class ConsoleTable : TextTable
{
        
    public List<ColorSelector> ColorSelectors { get; } 

    public class ColorSelector
    {
        private readonly Func<int,  string, bool> _matcher;

        public ColorSelector(Func<int,string,bool> matcher, ConsoleColor color)
        {
            _matcher = matcher;
            Color = color;
        }

        public bool Match(int col, string value)
        {
            return _matcher(col, value);
        }

        public ConsoleColor Color { get; }
    }

    public ConsoleTable(params string[] headings) : this(headings.ToList())
    { }


    public ConsoleTable(IEnumerable<string> headings) : base(new ConsoleTableWriter(new IndentingStringBuilder("  ")), headings)
    {
        ColorSelectors= new List<ColorSelector>();
    }

        
    public void Update(int row, int col, decimal val)
    {
        Update(row,col, $"{val:0.#}");
    }

    public void Update(int row, int col, long val)
    {
        var strVal = val.ToString();
        if (val == int.MinValue) strVal = "";
        if (val == int.MaxValue) strVal = "";
        Update(row,col,strVal);
    }

    public void Update(int row, int col, string val)
    {
        int top = ((ConsoleTableWriter)_writer).Top + 3 + row + row/5;
        int left = 1;
        for (int field = 0; field < col; field++)
        {
            left += _widths[field]+1;
        }
        int orgTop = Console.CursorTop;
        int orgLeft = Console.CursorLeft;
        Console.SetCursorPosition(left, top);
        Pad(val,' ', _widths[col]);
        Console.SetCursorPosition(orgLeft,orgTop);

    }
    private int RowHeight(List<string> row) => row.Select(s => s.LineCount()).Max();

    private class ConsoleTableWriter : TableWriter
    {
        public int Top { get; private set; }
        public override void Begin() => Top = Console.CursorTop;
        public override void End(){}
        public override void Write(string s)=> Console.Write(s);
        public override void Write(char c) => Console.Write(c);
        public override void WriteLine(char c) => Console.WriteLine(c);
        public ConsoleTableWriter(IndentingStringBuilder sb):base(sb)
        { }
    }
}



public abstract class TableWriter
{
    public abstract void Write(string s);
    public abstract void Write(char c);
    public abstract void WriteLine(char c);
    public abstract void Begin();
    public abstract void End();
    public IndentingStringBuilder Sb { get; }

    protected TableWriter(IndentingStringBuilder sb)
    {
        Sb = sb;
    }
}

public interface IHasTableRow
{
    IEnumerable<string> GetTableRow();
}