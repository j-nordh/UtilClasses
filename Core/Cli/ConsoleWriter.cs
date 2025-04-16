using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UtilClasses.Core.Extensions.Assemblies;
using UtilClasses.Core.Extensions.Booleans;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Cli;

public class ConsoleWriter
{
    private TextWriter _tw;
    public string Indent { get; set; }
    private bool isStartOfLine = true;
    public ConsoleColor SuccessColor { get; set; }
    public ConsoleColor ErrorColor { get; set; }

    public ConsoleWriter(TextWriter tw)
    {
        _tw = tw;
        Indent = "";
        SuccessColor = ConsoleColor.Green;
        ErrorColor = ConsoleColor.DarkRed;
    }

    public ConsoleWriter() : this(Console.Out)
    {
    }


    public ConsoleWriter Write(string s) => DoWrite(s);

    public ConsoleWriter Write(bool predicate, string s)
    {
        if (predicate)
            DoWrite(s);
        return this;
    }
    public ConsoleWriter WriteLine() => DoWriteLine("");
    public ConsoleWriter WriteLine(string s) => DoWriteLine(s);
    public ConsoleWriter WriteLine(object o) => DoWriteLine(o.ToString());

    public ConsoleWriter SetIndent(string s) => Do(() => Indent = s);

    public ConsoleWriter WriteResource(string identifier, bool newLine = true) =>
        WriteResource(identifier, Assembly.GetEntryAssembly(), newLine);
    public ConsoleWriter WriteResource(string identifier, Assembly ass, bool newLine)
    {
        var s = ass.GetResourceString(identifier);
        return newLine ? WriteLine(s) : Write(s);
    }
    public ConsoleWriter WithColor(ConsoleColor color, Action a)
    {
        var c = Console.ForegroundColor;
        Console.ForegroundColor = color;
        a();
        Console.ForegroundColor = c;
        return this;
    }
    private ConsoleWriter DoWrite(string s)
    {
        if (!s.Contains("\n"))
        {
            _tw.Write(isStartOfLine ? $"{Indent}{s}" : s);
            isStartOfLine = false;
            return this;
        }

        var lines = s.SplitLines();
        foreach (var l in lines.Leave(1))
        {
            DoWriteLine(l);
        }
        DoWrite(lines.Last());
        isStartOfLine = false;
        return this;
    }

    private ConsoleWriter DoWriteLine(string s)
    {
        foreach (var l in s.SplitLines())
        {
            DoWrite(l);
            _tw.WriteLine();
            isStartOfLine = true;
        }
        return this;
    }




    private ConsoleWriter Do(Action a)
    {
        a();
        return this;
    }
}
public static class ConsoleWriterExtensions
{
    public static ConsoleWriter WithColor(this ConsoleWriter wr, ConsoleColor color, Action<ConsoleWriter> a) =>
        wr.WithColor(color, () => a(wr));
    public static ConsoleWriter WithColor(this ConsoleWriter wr, ConsoleColor color, string s, bool newLine = true) =>
        wr.WithColor(color, () =>
            newLine.IfTrue(wr.WriteLine, wr.Write, s));

    public static ConsoleWriter Success(this ConsoleWriter wr, string s) => wr.WithColor(wr.SuccessColor, s);
    public static ConsoleWriter Error(this ConsoleWriter wr, string s) => wr.WithColor(wr.ErrorColor, s);
    public static ConsoleWriter Error(this ConsoleWriter wr, Exception e) => wr.WithColor(wr.ErrorColor, e.Message).WithColor(ConsoleColor.DarkGray,e.StackTrace);

    public static ConsoleWriter Attempt(this ConsoleWriter wr, string s, bool result)
    {
        wr.Write($"{s}: ");
        if (result)
            wr.WithColor(wr.SuccessColor, "OK");
        else
            wr.WithColor(wr.ErrorColor, "Failed");
        return wr;
    }
}