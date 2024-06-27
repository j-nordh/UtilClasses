using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.CodeGeneration;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Funcs;

namespace UtilClasses
{
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
        public static IndentingStringBuilder SourceFileBuilder(string declaration, string ns, string header, IEnumerable<string> usings) =>
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

        public IndentingStringBuilder Indent(bool gate, Func<IndentingStringBuilder, IndentingStringBuilder> f) => gate ? Indent(f) : this;

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
            strs.ForEach(s => Append(s));
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
        public IndentingStringBuilder MaybeAppendLines(bool condition, params string[] lines) => MaybeAppendLines(condition, lines.AsEnumerable());

        public IndentingStringBuilder MaybeAppendObject<T>(T o, Func<IndentingStringBuilder, T, IndentingStringBuilder> f) => null == o ? this : f(this, o);

        public IndentingStringBuilder Maybe(bool condition, params Func<IndentingStringBuilder, IndentingStringBuilder>[] fs)
        {
            if (condition)
                fs.ForEach(f => f(this));
            return this;
        }
        public IndentingStringBuilder MaybeOrNot(bool condition, Func<IndentingStringBuilder, IndentingStringBuilder> trueFunc, Func<IndentingStringBuilder, IndentingStringBuilder> falseFunc)
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
        public IndentingStringBuilder AppendObjects(IEnumerable<IAppendable> ps, bool extraNewLine = true) => AppendObjects(ps, string.Empty, extraNewLine);
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

        public IndentingStringBuilder AppendObjects<T>(IEnumerable<T> objs, Func<T, string> renderFunc, string? separator = null, bool extraNewLine = true)
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
        public IndentingStringBuilder AppendObjects<T>(IEnumerable<T>? objs, Func<T, int, string> renderFunc, string separator = "", bool extraNewLine = true)
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

        public IndentingStringBuilder AppendObjects<T>(IEnumerable<T>? objs, Action<IndentingStringBuilder, T> appender, string separator = "", bool extraNewLine = true)
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

        public IndentingStringBuilder AppendObjects(IEnumerable<ICodeElement> elements, string separator = "", bool extraNewLine = true) => AppendObjects(elements, e => e.AppendTo(this), separator, extraNewLine);

        public IndentingStringBuilder AppendObjects<T>(IEnumerable<T> objs, Action<T> appender, string separator, bool extraNewLine)
            => AppendObjects(objs, (_, p) => appender(p), separator, extraNewLine);
        public IndentingStringBuilder AppendObjects<T>(IEnumerable<T> objs, Action<T> appender, bool extraNewLine)
            => AppendObjects(objs, appender, "", extraNewLine);
        public IndentingStringBuilder AppendObjects<T>(IEnumerable<T> objs, Action<T> appender)
            => AppendObjects(objs, appender, "", true);

        public IndentingStringBuilder AppendObjects2<T>(IEnumerable<T> objs, Func<IndentingStringBuilder, T, IndentingStringBuilder> appender, string separator = "", bool extraNewLine = true)
            => AppendObjects(objs, (sb, o) => appender(sb, o), separator, extraNewLine);



        public IndentingStringBuilder AppendAppendables<T>(IEnumerable<T> objs, Func<T, IAppendable> conv, bool extraNewLine = true) => AppendObjects(objs.Select(conv), extraNewLine);


        public IndentingStringBuilder On<T>(Action<T> appender, bool extraNewLine = true) => AppendObjects(_objectDict[typeof(T)].Cast<T>(), appender, extraNewLine);
        public IndentingStringBuilder On<T>(Func<T, string> renderFunc, bool extraNewLine = true) => AppendObjects(_objectDict[typeof(T)].Cast<T>(), renderFunc, null, extraNewLine);
        public IndentingStringBuilder ForEach<T>(Action<IndentingStringBuilder, T> appender, bool extraNewLine = true) => AppendObjects(_objectDict[typeof(T)].Cast<T>(), appender, "", extraNewLine);

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

        public static TRet Wrap<TRet, T1>(Func<T1, IndentingStringBuilder, TRet> f, T1 arg1, out string str, string indentChars = "  ") =>
            Wrap(sb => f(arg1, sb), out str, indentChars);

        public static TRet Wrap<TRet, T1, T2>(Func<T1, T2, IndentingStringBuilder, TRet> f, T1 arg1, T2 arg2, out string str,
            string indentChars = "  ") =>
            Wrap(sb => f(arg1, arg2, sb), out str, indentChars);
        public static TRet Wrap<TRet, T1, T2, T3>(Func<T1, T2, T3, IndentingStringBuilder, TRet> f, T1 arg1, T2 arg2, T3 arg3, out string str,
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

    public static class IndentingStringBuilderExtensions
    {
        public static IEnumerable<IAppendable> AsAppendable(this IEnumerable<string> strings) => strings.Select(s => new StringAppendable(s));
        public static IndentingStringBuilder Inject(this IndentingStringBuilder sb, IEnumerable<Func<IndentingStringBuilder, IndentingStringBuilder>> fs)
        {
            foreach (var f in fs)
            {
                sb = f(sb);
            }
            return sb;
        }
        public static IndentingStringBuilder Inject(this IndentingStringBuilder sb, Func<IndentingStringBuilder, IndentingStringBuilder> f)
        {
            sb = f(sb);
            return sb;
        }
        public static IndentingStringBuilder Inject(this IndentingStringBuilder sb, IEnumerable<IInjector> injectors, Func<IInjector, Func<IndentingStringBuilder, IndentingStringBuilder>> f)
        => sb.Inject(injectors.Select(f));

        public static IndentingStringBuilder AppendAll(this IEnumerable<IAppendable> items, IndentingStringBuilder sb) => sb.AppendObjects(items);

        public static IAppendable Then(this IAppendable a1, IAppendable a2) => new MultiAppendable(a1, a2);
        public static string Render(this IAppendable a1, string indent = "  ")
        {
            var sb = new IndentingStringBuilder(indent);
            sb.AppendObject(a1);
            return sb.ToString();
        }

        public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, string line, Func<FunctionBuilder, FunctionBuilder> cfg) =>
            sb.AppendObject(cfg(FromLine(line)));
        public static IndentingStringBuilder AppendInlineFunction(this IndentingStringBuilder sb, string declaration, string line) =>
            sb.AppendObject(FromLine(declaration).Inline(line));
        public static IndentingStringBuilder AppendInlineFunction(this IndentingStringBuilder sb, string declaration, string parameters, string line) =>
            sb.AppendObject(FromLine(declaration).WithParameter(parameters).Inline(line));
        public static IndentingStringBuilder AppendInlineFunction(this IndentingStringBuilder sb, string declaration, IEnumerable<string> parameters, string line) =>
            sb.AppendObject(FromLine(declaration).WithParameters(parameters).Inline(line));
        public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, bool predicate, string line, Func<FunctionBuilder, FunctionBuilder> cfg) => predicate ? sb.AppendObject(cfg(FromLine(line))) : sb;

        public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, Func<FunctionBuilder, FunctionBuilder> cfg) =>
            sb.AppendObject(cfg(new FunctionBuilder()));
        public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, bool predicate, string line, string parameter, Func<FunctionBuilder, FunctionBuilder> cfg) => predicate ? sb.AppendFunction(line, parameter, cfg) : sb;

        public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, string line, string parameter, Func<FunctionBuilder, FunctionBuilder> cfg) =>
            sb.AppendObject(cfg(FromLine(line).WithParameter(parameter)));
        public static IndentingStringBuilder AppendFunction<T>(this IndentingStringBuilder sb, string line, string parameter, IEnumerable<T> objs, Func<T, string> formatter) =>
            sb.AppendObject(FromLine(line).WithParameter(parameter).WithBuilder(() => sb.AppendObjects(objs, formatter)));
        public static IndentingStringBuilder AppendFunction<T>(this IndentingStringBuilder sb, string line, string parameter, IEnumerable<T> objs, Action<IndentingStringBuilder, T> appender) =>
            sb.AppendObject(FromLine(line).WithParameter(parameter).WithBuilder(() => sb.AppendObjects(objs, appender)));
        public static IndentingStringBuilder AppendForEach(this IndentingStringBuilder sb, string varName, string col, params string[] lines) => sb
            .AppendLines($"foreach (var {varName} in {col})", "{")
            .AppendLines(lines)
            .AppendLine("}");
        public static IndentingStringBuilder AppendForEach(this IndentingStringBuilder sb, string varName, string col, Func<IndentingStringBuilder, IndentingStringBuilder> f) =>
            f(sb.AppendLines($"foreach (var {varName} in {col})", "{")).AppendLine("}");
        public static void Add(this List<IAppendable> lst, string s) => lst.Add(new StringAppendable(s));

        private static FunctionBuilder FromLine(string line)
        {
            var ret = new FunctionBuilder();
            var parts = line?.SplitRemoveEmptyEntries(" ") ?? new string[] { };
            if (parts.Count() == 1)
                ret.Name = parts[0];
            if (parts.Count() == 2)
            {
                ret.Returns = parts[0];
                ret.Name = parts[1];
            }
            if (parts.Count() == 3)
            {
                ret.Modifier = parts[0];
                ret.Returns = parts[1];
                ret.Name = parts[2];
            }
            return ret;
        }


        public static FunctionBuilder WithModifier(this FunctionBuilder fb, string mod) => fb.Do(() => fb.Modifier = mod);
        public static FunctionBuilder Returning(this FunctionBuilder fb, string ret) => fb.Do(() => fb.Returns = ret);
        public static FunctionBuilder ReturningList(this FunctionBuilder fb) => fb.Do(() => fb.Returns = $"List<{fb.Returns}>");
        public static FunctionBuilder ReturningList(this FunctionBuilder fb, string ret) => fb.Do(() => fb.Returns = $"List<{ret}>");
        public static FunctionBuilder WithName(this FunctionBuilder fb, string name) => fb.Do(() => fb.Name = name);
        public static FunctionBuilder InsertParameter(this FunctionBuilder fb, string p) => fb.Do(() => fb.Parameters.Insert(0, p));
        public static FunctionBuilder InsertParameter(this FunctionBuilder fb, int index, string p) => fb.Do(() => fb.Parameters.Insert(index, p));
        public static FunctionBuilder WithParameter(this FunctionBuilder fb, string p) => fb.Do(() => fb.Parameters.Add(p));

        public static FunctionBuilder WithoutOptionalParameters(this FunctionBuilder fb) => fb.Do(() =>
            fb.Parameters = fb.Parameters.Select(p => p.SubstringBefore("=")).ToList());
        public static FunctionBuilder WithParameters(this FunctionBuilder fb, IEnumerable<string> ps) => fb.Do(() => fb.Parameters.AddRange(ps));
        public static FunctionBuilder WithParameters(this FunctionBuilder fb, params string[] ps) => fb.Do(() => fb.Parameters.AddRange(ps));

        public static FunctionBuilder WithBuilder(this FunctionBuilder fb, Func<IndentingStringBuilder, IndentingStringBuilder> f) => fb.Do(() => fb.BodyBuilders.Add(f));
        public static FunctionBuilder WithBuilder(this FunctionBuilder fb, Func<IndentingStringBuilder> f) => fb.Do(() => fb.BodyBuilders.Add(_ => f()));
        public static FunctionBuilder WithBuilder(this FunctionBuilder fb, Action<IndentingStringBuilder> a) => fb.Do(() => fb.BodyBuilders.Add(isb => { a(isb); return isb; }));
        public static FunctionBuilder WithBuilder(this FunctionBuilder fb, Action a) => fb.Do(() => fb.BodyBuilders.Add(isb => { a(); return isb; }));
        public static FunctionBuilder ClearBuilders(this FunctionBuilder fb) => fb.Do(() => fb.BodyBuilders.Clear());
        public static FunctionBuilder With<T>(this FunctionBuilder fb, IEnumerable<T> objects, Func<T, string> formatter) => fb.WithBuilder(sb => sb.AppendObjects(objects, formatter));
        public static FunctionBuilder With<T>(this FunctionBuilder fb, IEnumerable<T> objects, Action<IndentingStringBuilder, T> appender) => fb.WithBuilder(sb => sb.AppendObjects(objects, appender));
        public static FunctionBuilder With(this FunctionBuilder fb, params string[] lines) => fb.WithBuilder(sb => sb.AppendLines(lines));


        public static FunctionBuilder WithObj(this FunctionBuilder fb, string type) => fb.WithParameter($"{type} obj").Do(() => fb.HasObj = true);
        public static FunctionBuilder WithObjs(this FunctionBuilder fb, string type) => fb.WithParameter($"IEnumerable<{type}> objs").Do(() => fb.HasObjs = true);
        public static FunctionBuilder Inline(this FunctionBuilder fb) => fb.Do(() => fb.Inline = true);
        public static FunctionBuilder Inline(this FunctionBuilder fb, string line) => fb.ClearBuilders().Inline().With(line);
        public static FunctionBuilder Inline(this FunctionBuilder fb, params string[] lines) => fb
            .ClearBuilders()
            .Inline()
            .WithBuilder(b => b
                .AppendLine(lines[0])
                .Maybe(lines.Count() > 1,
                    () => b.Indent().AppendLines(lines.Skip(1)).Outdent()));
        public static IEnumerable<FunctionBuilder> Split(this FunctionBuilder fb, params Func<FunctionBuilder, FunctionBuilder>[] cfgs)
        {
            foreach (var cfg in cfgs)
            {
                yield return cfg(new FunctionBuilder(fb));
            }
        }
    }

    public class FunctionBuilder : IAppendable
    {
        public string Modifier { get; set; }
        public string Returns { get; set; }
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public List<Func<IndentingStringBuilder, IndentingStringBuilder>> BodyBuilders { get; set; }

        public bool Inline { get; set; }
        public bool HasObj { get; set; }
        public bool HasObjs { get; set; }
        public FunctionBuilder()
        {
            Modifier = "public";
            Returns = "void";
            Parameters = new List<string>();
            BodyBuilders = new List<Func<IndentingStringBuilder, IndentingStringBuilder>>();
            Inline = false;
        }

        public FunctionBuilder(FunctionBuilder o)
        {
            Modifier = o.Modifier;
            Name = o.Name;
            Returns = o.Returns;
            Parameters = new List<string>(o.Parameters);
            BodyBuilders = new List<Func<IndentingStringBuilder, IndentingStringBuilder>>(o.BodyBuilders);
            Inline = o.Inline;
        }

        public FunctionBuilder Overload()
        {
            var ret = new FunctionBuilder(this);
            ret.BodyBuilders.Clear();
            return ret;
        }

        public IndentingStringBuilder AppendObject(IndentingStringBuilder sb)
        {
            if (Inline)
            {
                return sb.AppendLine($"{Modifier} {Returns} {Name}({Parameters?.Join(", ") ?? ""}) =>")
                    .Indent(x => x.Inject(BodyBuilders));
            }
            else
            {
                return sb
                    .AppendLines($"{Modifier} {Returns} {Name}({Parameters?.Join(", ") ?? ""})", "{")
                    .MaybeAppendLines(HasObjs, "if (null == objs) return null;",
                        "var lst = objs.SmartToList();",
                        "if (!lst.Any()) return lst;")
                    .Inject(BodyBuilders)
                    .MaybeAppendLine(HasObj, "return obj;")
                    .MaybeAppendLine(HasObjs, "return lst;")
                    .AppendLine("}");
            }

        }
    }

}
