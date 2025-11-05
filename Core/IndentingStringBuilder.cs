using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Core.CodeGeneration;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Objects;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core;

public class IndentingStringBuilder
{
    private int _indent;
    private readonly StringBuilder _sb;
    private bool _isNewLine;
    private bool _isBlocked;
    private readonly HashSet<string> _autoIndentMarkers;
    private readonly HashSet<string> _autoOutdentMarkers;
    private readonly Dictionary<Type, List<object>> _objectDict;
    private bool _isFinalized;
    private Action<IndentingStringBuilder> _finalizer;
    private int _closeOnFinalize = 0;

    public string IndentChars { get; set; }

    public IndentingStringBuilder(string indentChars, int initialLevel = 0)
    {
        IndentChars = indentChars;
        _indent = initialLevel;
        _sb = new StringBuilder();
        _isNewLine = true;
        _autoIndentMarkers = new HashSet<string>();
        _autoOutdentMarkers = new HashSet<string>();
        _objectDict = new Dictionary<Type, List<object>>();
    }

    public int Length => _sb.Length;

    public static IndentingStringBuilder SourceFileBuilder(string declaration, string ns, string header) =>
        SourceFileBuilder(declaration, ns, header, new string[] { });

    public static IndentingStringBuilder SourceFileBuilder(string declaration, string ns, string header,
        IEnumerable<string> usings) =>
        new IndentingStringBuilder("\t")
            .AutoIndentOnCurlyBraces()
            .AppendLines(usings.Select(u => $"using {u};"))
            .AppendLine(header)
            .AppendLines($"namespace {ns}", "{", declaration, "{")
            .Do(x => x._closeOnFinalize = 2)
            .WithFinalizer(last => last.AppendLine("}", last._closeOnFinalize));


    public IndentingStringBuilder WithFinalizer(Action<IndentingStringBuilder> finalizer)
    {
        _finalizer = finalizer;
        return this;
    }

    public IndentingStringBuilder Close(string closer = "}", int count = 1) => AppendLine(closer).Close(count);
    private IndentingStringBuilder Close(int count) => this.Do(() => _closeOnFinalize -= count);

    public IndentingStringBuilder RunFinalizer()
    {
        if (_isFinalized) return this;
        if (_finalizer == null) return this;
        _finalizer(this);
        _isFinalized = true;
        return this;
    }

    public IndentingStringBuilder Indent() => Indent(1);

    public IndentingStringBuilder Indent(int steps, Action a)
    {
        Indent(steps);
        a();
        Outdent(steps);
        return this;
    }

    public IndentingStringBuilder Enclose(string start, string stop, Action<IndentingStringBuilder> a)
    {
        AppendLine(start);
        Indent();
        a(this);
        Outdent();
        AppendLine(stop);
        return this;
    }

    public IndentingStringBuilder Enclose_Brackets(Action<IndentingStringBuilder> a) => Enclose("[", "]", a);

    public IndentingStringBuilder Indent(Func<IndentingStringBuilder, IndentingStringBuilder> f)
    {
        Indent();
        f(this);
        Outdent();
        return this;
    }

    public IndentingStringBuilder Indent(Action a)
    {
        Indent();
        a();
        return Outdent();
    }

    public IndentingStringBuilder Indent(Action<IndentingStringBuilder> a)
    {
        Indent();
        a(this);
        return Outdent();
    }

    public IndentingStringBuilder Indent(bool gate, Func<IndentingStringBuilder, IndentingStringBuilder> f) =>
        gate ? Indent(f) : this;

    public IndentingStringBuilder Indent(bool gate, Action<IndentingStringBuilder> a) => Indent(gate, s =>
    {
        a(s);
        return this;
    });


    public IndentingStringBuilder Outdent() => Indent(-1);
    public IndentingStringBuilder Outdent(int steps) => Indent(-steps);

    public IndentingStringBuilder Indent(int steps)
    {
        if (_isBlocked) return this;
        _indent += steps;
        if (_indent < 0) _indent = 0;
        return this;
    }

    public IndentingStringBuilder SetIndentChars(string s) => this.Do(() => IndentChars = s);

    public IndentingStringBuilder AppendLine(char c) => AppendLine(c.ToString());

    public IndentingStringBuilder AppendLine(string line)
    {
        if (_isBlocked) return this;
        if (null == line) return this;
        var lines = line.SplitLines();
        foreach (var s in lines)
        {
            HandleAutoOutdent(s);
            ApplyIndent();
            _sb.AppendLine(s);
            HandleAutoIndent(s);
            _isNewLine = true;
        }

        return this;
    }

    public IndentingStringBuilder AppendLine(string line, int repeat)
    {
        for (int i = 0; i < repeat; i++)
        {
            AppendLine(line);
        }

        return this;
    }

    public IndentingStringBuilder IndentLine(string line)
    {
        Indent();
        AppendLine(line);
        Outdent();
        return this;
    }

    public IndentingStringBuilder AppendLine()
    {
        _sb.Append(Environment.NewLine);
        _isNewLine = true;
        return this;
    }

    public IndentingStringBuilder Append(char c)
    {
        if (_isBlocked) return this;
        ApplyIndent();
        _sb.Append(c);
        return this;
    }

    public IndentingStringBuilder Append(char c, int repeat)
    {
        if (_isBlocked) return this;
        ApplyIndent();
        _sb.Append(c, repeat);
        return this;
    }

    public IndentingStringBuilder Append(params string[] strs) => Append(strs.AsEnumerable());

    public IndentingStringBuilder Append(IEnumerable<string> strs)
    {
        strs.ForEach(s => Append((string)s));
        return this;
    }

    public IndentingStringBuilder Append(string str)
    {
        if (_isBlocked) return this;
        var lines = str.SplitLines();
        if (!lines.Any()) return this;
        foreach (var line in lines.Leave(1))
        {
            AppendLine(line);
        }

        ApplyIndent();
        var last = lines.Last();
        HandleAutoOutdent(last);
        _sb.Append(last);
        HandleAutoIndent(last);
        return this;
    }

    public IndentingStringBuilder Append(Action<IndentingStringBuilder> a)
    {
        a(this);
        return this;
    }

    public IndentingStringBuilder MaybeAppend(bool condition, string str)
    {
        if (condition) Append(str);
        return this;
    }

    public IndentingStringBuilder MaybeAppendLine(bool condition, string str)
    {
        if (condition) AppendLine(str);
        return this;
    }

    public IndentingStringBuilder MaybeAppendLine(string str)
    {
        if (!str.IsNullOrWhitespace()) AppendLine(str);
        return this;
    }

    public IndentingStringBuilder MaybeAppendLines(bool condition, IEnumerable<string> lines)
    {
        if (condition) AppendLines(lines);
        return this;
    }

    public IndentingStringBuilder MaybeAppendLines(bool condition, params string[] lines) =>
        MaybeAppendLines(condition, lines.AsEnumerable());

    public IndentingStringBuilder MaybeAppendLines(bool condition, string heading, params string[] lines)
    {
        if (!condition) return this;
        AppendLine(heading);
        AppendLines(lines.AsEnumerable());
        return this;
    }

    public IndentingStringBuilder
        MaybeAppendObject<T>(T o, Func<IndentingStringBuilder, T, IndentingStringBuilder> f) =>
        null == o ? this : f(this, o);

    public IndentingStringBuilder Maybe(bool condition,
        params Func<IndentingStringBuilder, IndentingStringBuilder>[] fs)
    {
        if (condition)
            fs.ForEach(f => f(this));
        return this;
    }

    public IndentingStringBuilder MaybeOrNot(bool condition,
        Func<IndentingStringBuilder, IndentingStringBuilder> trueFunc,
        Func<IndentingStringBuilder, IndentingStringBuilder> falseFunc)
    {
        if (condition) trueFunc(this);
        else falseFunc(this);
        return this;
    }

    public IndentingStringBuilder Maybe(bool condition, Action a)
    {
        if (condition) a();
        return this;
    }

    public IndentingStringBuilder Maybe(bool condition, Func<IndentingStringBuilder> f)
    {
        return condition ? f() : this;
    }

    public IndentingStringBuilder AppendLines(IEnumerable<string> strs)
    {
        if (null == strs) return this;
        foreach (var str in strs)
        {
            AppendLine(str);
        }

        return this;
    }

    public IndentingStringBuilder AppendLines(params string[] arr)
    {
        arr.ForEach(s => AppendLine(s));
        return this;
    }

    public IndentingStringBuilder AppendLines(string heading, params string[] arr)
    {
        AppendLine(heading);
        arr.ForEach(s => AppendLine(s));
        return this;
    }


    public IndentingStringBuilder Encapsulate(string marker, string content) => AppendLines(
        $"------------- {marker} Start -------------",
        content,
        $"-------------- {marker} End --------------");

    public IndentingStringBuilder AppendLines(IEnumerable<string> strs, string separator)
    {
        var lst = strs as List<string> ?? strs.ToList();
        if (!lst.Any())
        {
            return this;
        }

        foreach (var line in lst.Leave(1))
        {
            Append(line);
            Append(separator);
            AppendLine();
        }

        AppendLine(lst.Last());
        return this;
    }

    private void ApplyIndent()
    {
        if (!_isNewLine || _isBlocked) return;
        _isNewLine = false;
        for (int i = 0; i < _indent; i++)
        {
            _sb.Append(IndentChars);
        }
    }

    public IndentingStringBuilder Tab()
    {
        if (_isBlocked) return this;
        ApplyIndent();
        _sb.Append("\t");
        return this;
    }

    public override string ToString()
    {
        RunFinalizer();
        return _sb.ToString();
    }

    public IndentingStringBuilder PrevLine()
    {
        if (_isBlocked) return this;
        bool foundNewLine = false;
        int i = _sb.Length - 1;
        for (; i >= 0; i--)
        {
            if (_sb[i] == '\n') foundNewLine = true;
            else if (_sb[i] == '\r') foundNewLine = true;
            else
            {
                if (foundNewLine) break;
            }
        }

        if (i <= 0) return this;
        i += 1;
        _sb.Remove(i, _sb.Length - i);
        _isNewLine = false;
        return this;
    }

    public IndentingStringBuilder Backspace(int steps = 1)
    {
        if (_isBlocked) return this;
        _sb.Remove(_sb.Length - steps, steps);
        return this;
    }

    public IndentingStringBuilder AppendObject(IAppendable o)
    {
        o?.AppendObject(this);
        return this;
    }

    public IndentingStringBuilder AppendObjects(IEnumerable<IAppendable> ps, bool extraNewLine = true) =>
        AppendObjects(ps, string.Empty, extraNewLine);

    public IndentingStringBuilder AppendObjects(IEnumerable<IAppendable> ps, string separator, bool extraNewLine = true)
    {
        if (ps.IsNullOrEmpty()) return this;

        foreach (var p in ps.Leave(1))
        {
            AppendObject(p);
            Append(separator);
            if (extraNewLine) AppendLine();
        }

        AppendObject(ps.Last());
        if (extraNewLine) AppendLine();
        return this;
    }

    public IndentingStringBuilder AppendObjects<T>(IEnumerable<T> objs, Func<T, string> renderFunc,
        string? separator = null, bool extraNewLine = true)
    {
        if (objs.IsNullOrEmpty()) return this;
        foreach (var o in objs.Leave(1))
        {
            Append(renderFunc(o));
            if (separator.IsNotNullOrEmpty()) Append(separator);
            if (extraNewLine) AppendLine();
        }

        Append(renderFunc(objs.Last()));
        if (extraNewLine) AppendLine();
        return this;
    }

    public IndentingStringBuilder AppendObjects<T>(IEnumerable<T>? objs, Func<T, int, string> renderFunc,
        string separator = "", bool extraNewLine = true)
    {
        if (null == objs) return this;
        var lst = objs.ToList();
        if (!lst.Any()) return this;

        for (int i = 0; i < lst.Count; i++)
        {
            Append(renderFunc(lst[i], i));
            if (i < lst.Count - 1 && separator.IsNotNullOrEmpty()) Append(separator);
            if (extraNewLine) AppendLine();
        }

        return this;
    }

    public IndentingStringBuilder AppendObjects<T>(IEnumerable<T>? objs, Action<IndentingStringBuilder, T> appender,
        string separator = "", bool extraNewLine = true)
    {
        if (null == objs)
            return this;
        var queue = new Queue<T>(objs);
        if (!queue.Any())
            return this;
        while (queue.Any())
        {
            var o = queue.Dequeue();
            appender(this, o);
            if (queue.Any() && separator.IsNotNullOrEmpty()) Append(separator);
            if (extraNewLine) AppendLine();
        }

        return this;
    }

    public IndentingStringBuilder AppendObjects(IEnumerable<ICodeElement> elements, string separator = "",
        bool extraNewLine = true) => AppendObjects(elements, e => e.AppendTo(this), separator, extraNewLine);

    public IndentingStringBuilder AppendObjects<T>(IEnumerable<T> objs, Action<T> appender, string separator,
        bool extraNewLine)
        => AppendObjects(objs, (_, p) => appender(p), separator, extraNewLine);

    public IndentingStringBuilder AppendObjects<T>(IEnumerable<T> objs, Action<T> appender, bool extraNewLine)
        => AppendObjects(objs, appender, "", extraNewLine);

    public IndentingStringBuilder AppendObjects<T>(IEnumerable<T> objs, Action<T> appender)
        => AppendObjects(objs, appender, "", true);

    public IndentingStringBuilder AppendObjects2<T>(IEnumerable<T> objs,
        Func<IndentingStringBuilder, T, IndentingStringBuilder> appender, string separator = "",
        bool extraNewLine = true)
        => AppendObjects(objs, (sb, o) => appender(sb, o), separator, extraNewLine);


    public IndentingStringBuilder AppendAppendables<T>(IEnumerable<T> objs, Func<T, IAppendable> conv,
        bool extraNewLine = true) => AppendObjects(objs.Select(conv), extraNewLine);

    public IndentingStringBuilder AppendObjects(bool predicate, IEnumerable<IAppendable> ps, bool extraNewLine = true)
        => predicate
            ? AppendObjects(ps, string.Empty, extraNewLine)
            : this;

    public IndentingStringBuilder AppendObjects(bool predicate, string heading, IEnumerable<IAppendable> ps,
        bool extraNewLine = true)
    {
        if (!predicate)
            return this;
        AppendLine(heading);
        return AppendObjects(ps, string.Empty, extraNewLine);
    }

    public IndentingStringBuilder On<T>(Action<T> appender, bool extraNewLine = true) =>
        AppendObjects(_objectDict[typeof(T)].Cast<T>(), appender, extraNewLine);

    public IndentingStringBuilder On<T>(Func<T, string> renderFunc, bool extraNewLine = true) =>
        AppendObjects(_objectDict[typeof(T)].Cast<T>(), renderFunc, null, extraNewLine);

    public IndentingStringBuilder ForEach<T>(Action<IndentingStringBuilder, T> appender, bool extraNewLine = true) =>
        AppendObjects(_objectDict[typeof(T)].Cast<T>(), appender, "", extraNewLine);

    public IndentingStringBuilder AppendLines_ToString(IEnumerable<object> os, string nullValue = "-Null-")
    {
        foreach (var o in os)
        {
            AppendLine(o?.ToString() ?? nullValue);
        }

        return this;
    }
    


    public IndentingStringBuilder OutdentLine(string line) => OutdentLines(new[] { line });
    public IndentingStringBuilder OutdentLines(params string[] lines) => OutdentLines((IEnumerable<string>)lines);

    public IndentingStringBuilder OutdentLines(IEnumerable<string> lines)
    {
        Outdent();
        AppendLines(lines);
        Indent();
        return this;
    }

    public void Clear()
    {
        if (_isBlocked) return;
        _sb.Clear();
        _indent = 0;
    }

    public IndentingStringBuilder BlockIf(bool b)
        => Block(b || _isBlocked);

    public IndentingStringBuilder Block(bool b = true)
    {
        _isBlocked = b;
        return this;
    }

    public IndentingStringBuilder UnBlock()
    {
        _isBlocked = false;
        return this;
    }

    public IndentingStringBuilder AutoIndentOn(string s)
    {
        _autoIndentMarkers.Add(s);
        return this;
    }

    public IndentingStringBuilder AutoIndentOn(string sIn, string sOut)
    {
        _autoIndentMarkers.Add(sIn);
        _autoOutdentMarkers.Add(sOut);
        return this;
    }

    public IndentingStringBuilder AutoIndentOnCurlyBraces() => AutoIndentOn("{", "}");


    public IndentingStringBuilder AutoOutdentOn(string s)
    {
        _autoOutdentMarkers.Add(s);
        return this;
    }

    private void HandleAutoIndent(string s)
    {
        if (_autoIndentMarkers.Count == 0 || !_autoIndentMarkers.Contains(s)) return;
        Indent();
    }

    private void HandleAutoOutdent(string s)
    {
        if (_autoOutdentMarkers.Count == 0 || !_autoOutdentMarkers.Contains(s)) return;
        Outdent();
    }

    public IndentingStringBuilder OperatingOn<T>(IEnumerable<T> items)
    {
        _objectDict[typeof(T)] = items.Cast<object>().ToList();
        return this;
    }


    public static TRet Wrap<TRet>(Func<IndentingStringBuilder, TRet> f, out string str, string indentChars = "  ")
    {
        var sb = new IndentingStringBuilder(indentChars);
        var ret = f(sb);
        str = sb.ToString();
        return ret;
    }

    public static TRet Wrap<TRet, T1>(Func<T1, IndentingStringBuilder, TRet> f, T1 arg1, out string str,
        string indentChars = "  ") =>
        Wrap(sb => f(arg1, sb), out str, indentChars);

    public static TRet Wrap<TRet, T1, T2>(Func<T1, T2, IndentingStringBuilder, TRet> f, T1 arg1, T2 arg2,
        out string str,
        string indentChars = "  ") =>
        Wrap(sb => f(arg1, arg2, sb), out str, indentChars);

    public static TRet Wrap<TRet, T1, T2, T3>(Func<T1, T2, T3, IndentingStringBuilder, TRet> f, T1 arg1, T2 arg2,
        T3 arg3, out string str,
        string indentChars = "  ") =>
        Wrap(sb => f(arg1, arg2, arg3, sb), out str, indentChars);
}

public interface IAppendable
{
    IndentingStringBuilder AppendObject(IndentingStringBuilder sb);
}

public abstract class Appendable<T> : IAppendable
{
    protected readonly T _obj;

    protected Appendable(T obj)
    {
        _obj = obj;
    }

    public abstract IndentingStringBuilder AppendObject(IndentingStringBuilder sb);
    public virtual string Render() => new IndentingStringBuilder("\t").AppendObject(this).ToString();
    public override string ToString() => typeof(T).Name;
}

public class StringAppendable : IAppendable
{
    private readonly string _str;

    public StringAppendable(string str)
    {
        _str = str;
    }

    public IndentingStringBuilder AppendObject(IndentingStringBuilder sb) => sb.Append(_str);
}

public class FuncAppendable : IAppendable
{
    private readonly Func<IndentingStringBuilder, IndentingStringBuilder> _f;

    public FuncAppendable(Func<IndentingStringBuilder, IndentingStringBuilder> f)
    {
        _f = f;
    }

    public FuncAppendable(Action<IndentingStringBuilder> a)
    {
        _f = sb =>
        {
            a(sb);
            return sb;
        };
    }

    public IndentingStringBuilder AppendObject(IndentingStringBuilder sb) => _f(sb);
}

public class MultiAppendable : IAppendable
{
    private readonly List<IAppendable> _apps;
    private readonly string? _separator;

    public MultiAppendable(IEnumerable<IAppendable> apps, string? separator = null)
    {
        if (null == apps)
            throw new ArgumentNullException(nameof(apps));
        _apps = apps.ToList();
        _separator = separator ?? "";
    }

    public MultiAppendable(params IAppendable[] apps)
    {
        _apps = apps.ToList();
    }

    public IndentingStringBuilder AppendObject(IndentingStringBuilder sb)
    {
        foreach (var app in _apps.Leave(1))
        {
            app.AppendObject(sb);
            if (_separator.IsNotNullOrEmpty()) sb.Append(_separator);
        }

        if (_apps.Count >= 1)
            _apps.Last().AppendObject(sb);
        return sb;
    }
}