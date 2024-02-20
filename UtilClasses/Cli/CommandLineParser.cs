using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Expressions;
using UtilClasses.Extensions.Integers;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Cli
{
    public class CommandLineParser
    {
        protected readonly Dictionary<string, Action<string>> _settings = new(StringComparer.OrdinalIgnoreCase);
        protected readonly Dictionary<string, Action> _flags= new(StringComparer.OrdinalIgnoreCase);
        protected readonly Dictionary<string, Action> _verbs= new(StringComparer.OrdinalIgnoreCase);
        protected readonly Dictionary<string, string> _parseResult= new(StringComparer.OrdinalIgnoreCase);
        protected readonly List<(ArgumentType Type, Func<List<string>, bool> Predicate, Action Action)> _preConditions = new();
        protected readonly List<(Func<Dictionary<string, string>, bool> Predicate, Action Action)> _parsedConditions= new();

        private readonly Dictionary<string, List<string>> _errors = new();
        private string? _defaultVerb;

        public void AddSetting(string name, Action<string> setter) => _settings[name] = setter;
        public void AddFlag(string name, Action onFlag) => _flags[name] = onFlag;
        public void AddVerb(string name, Action onVerb) => _verbs[name] = onVerb;
        public void SetDefaultVerb(string name)
        {
            if (!_verbs.ContainsKey(name))
                throw new ArgumentException("The supplied verb name does not match a specified verb.");
            _defaultVerb = name;
        }

        public List<string> Unmatched { get; private set; }

        public void AddPreCondition(ArgumentType t, Func<List<string>, bool> predicate, string header) => 
            _preConditions.Add((t, predicate, () => _errors.GetOrAdd(header)));

        public void AddParsedCondition(Func<Dictionary<string, string>, bool> predicate, string header) => 
            _parsedConditions.Add((predicate, () => _errors.GetOrAdd(header)));

        public ArgumentType GetArgType(string a)
        {
            if (a.Contains("="))
            {
                a = a.SubstringBefore("=");
            }
            a = a.Trim('-', '/', ' ');
            return _settings.ContainsKey(a)
                ? ArgumentType.Setting
                : _flags.ContainsKey(a)
                    ? ArgumentType.Flag
                    : _verbs.ContainsKey(a)
                        ? ArgumentType.Verb
                        : ArgumentType.Unmatched;
        }

        private Func<string, bool> GetTypeFilter(ArgumentType t) => s => ((int)GetArgType(s) & (int)t) > 0;

        public CommandLineParser Parse() => Parse(Environment.GetCommandLineArgs().Skip(1).ToList());

        private void ThrowErrors()
        {
            if (!_errors.Any()) return;
            var sb = new IndentingStringBuilder("t");
            foreach (var k in _errors.Keys)
            {
                sb.AppendLine(k).Indent(() => sb.AppendLines(_errors[k]));
            }

            throw new ArgumentException(sb.ToString());
        }
        public CommandLineParser Parse(IEnumerable<string> args)
        {
            Unmatched = new List<string>();
            var verbQueue = new Queue<string>();
            var lst = args.ToList();
            foreach (var c in _preConditions)
            {
                if (!c.Predicate(lst.Where(GetTypeFilter(c.Type)).ToList()))
                    c.Action();
            }
            ThrowErrors();

            foreach (var arg in lst)
            {
                if (arg.Contains("="))
                {
                    arg.SplitAssign("=", out var name, out var value);
                    name = name.Trim('-', '/', ' ');
                    _parseResult[name] = value;
                    _settings.Maybe(name)?.Invoke(value);
                }
                else if (arg.StartsWith("/") || arg.StartsWith("-"))
                {
                    var name = arg.Trim('-', '/', ' ');
                    _flags.Maybe(name)?.Invoke();
                    _parseResult[name] = "true";
                }
                else
                {
                    var name = arg.Trim('-', '/', ' ');
                    verbQueue.Enqueue(name);
                    _parseResult[name] = "queued";
                }
            }

            _parsedConditions
                .Where(c => !c.Predicate(_parseResult))
                .Select(c => c.Action)
                .RunAll();

            ThrowErrors();

            if (_defaultVerb.IsNotNullOrEmpty() && !verbQueue.Any())
            {
                _verbs[_defaultVerb]();
                return this;
            }
            while (verbQueue.Any())
            {
                var part = verbQueue.Dequeue();
                var v = _verbs.Maybe(part);
                if (null != v)
                {
                    v();
                    continue;
                }
                Unmatched.Add(part);
            }

            return this;
        }

    }

    public class CommandLineParser<T> : CommandLineParser
    {
        private readonly T _o;
        public CommandLineParser(T o)
        {
            if (null == o) throw new ArgumentException("Object reference must not be null");
            _o = o;
        }

        public CommandLineParser WithFlag(string name, Expression<Func<T, bool>> expr)
        {
            _flags[name] = () => expr.Set(_o, true);
            return this;
        }
        public CommandLineParser<T> With(string name, Expression<Func<T, string>> expr, bool mandatory = false) => Setting(name, s => expr.Set(_o, s), mandatory);
        public CommandLineParser<T> With(string name, Expression<Func<T, List<string>>> expr, bool mandatory = false) => Setting(name, str =>
        {
            var lst = expr.Get<List<string>>(_o);
            if (null == lst)
            {
                lst = new List<string>();
                expr.Set(_o,lst);
            }
            lst.AddRange(str.Split(',', ';'));
        }, mandatory);

        private CommandLineParser<T> Setting(string name, Action<string> setter, bool mandatory)
        {
            _settings[name] = setter;
            if (mandatory)
                this.RequireSetting(name);
            return this;
        }
        public CommandLineParser<T> With(string name, Expression<Func<T, DateTime?>> expr, bool mandatory = false) =>
            Setting(name, s => expr.Set(_o, s.AsDateTime()), mandatory);

        public CommandLineParser<T> With(string name, Expression<Func<T, int>> expr, bool mandatory = false) =>
            Setting(name, s => expr.Set(_o, s.AsInt()), mandatory);
    }

    public static class CommandLineParserExtensions
    {
        public static TClp With<TClp>(this TClp clp, string name, Action<string> setter) where TClp : CommandLineParser
        {
            clp.AddSetting(name, setter);
            return clp;
        }

        public static TClp WithFlag<TClp>(this TClp clp, string name, Action onFlag) where TClp : CommandLineParser
        {
            clp.AddFlag(name, onFlag);
            return clp;
        }

        public static TClp WithVerb<TClp>(this TClp clp, string name, Action onVerb, bool isDefault) where TClp : CommandLineParser
        {
            clp.AddVerb(name, onVerb);
            if (isDefault)
                clp.SetDefaultVerb(name);
            return clp;
        }

        public static TClp WithPreCondition<TClp>(this TClp clp, ArgumentType t, Func<List<string>, bool> predicate, string header) where TClp : CommandLineParser
        {
            clp.AddPreCondition(t, predicate, header);
            return clp;
        }

        public static TClp RequireSetting<TClp>(this TClp clp, string name) where TClp : CommandLineParser
        {
            clp.AddParsedCondition(res => res.ContainsKey(name), $"The required setting \"{name}\" is missing");
            return clp;
        }
    }

    [Flags]
    public enum ArgumentType
    {
        None,
        Flag = 1,
        Setting = 2,
        Verb = 4,
        Unmatched = 8,
        All = 15
    }
}
