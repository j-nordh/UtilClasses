using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Types;
using static System.Char;

namespace UtilClasses.Cli
{
    public class ConsoleUtil
    {
        public static readonly char[] Options = new[]
        {
            '1', '2', '3', '4', '5', '6', '7', '8',
            '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
            'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
            'p', 'r', 's', 't', 'u', 'v', 'y', 'z'
        };
        public static char Prompt(string prompt, params char[] allowedAnswers) =>
            Prompt(prompt, allowedAnswers.AsEnumerable());
        public static char Prompt(string prompt, IEnumerable<char> allowedAnswers)
        {
            var answers = allowedAnswers?.ToList();
            if (answers == null || answers.Count == 0)
            {
                answers = new List<char> { 'y', 'n' };
            }

            answers = answers.Select(ToLower).ToList();
            char answer = Enumerable.Range(MinValue, MaxValue).Select(c => (char)c).First(c => !answers.Contains(c));
            while (!answers.Contains(answer))
            {
                Console.Write(prompt);
                answer = ToLower(Console.ReadKey().KeyChar);
                Console.WriteLine();
            }

            return answer;
        }

        public static Option Prompt(string prompt, IEnumerable<Option> opts)
        {
            var lst = opts.ToList();
            var sb = new IndentingStringBuilder("  ").AppendLine(prompt);

            lst.ForEach(opt => sb.AppendLine($"{opt.Key}) {opt.Caption}"));
            var res = Prompt(sb.ToString(), lst.Select(opt => opt.Key));

            return lst.First(o=>o.Key == res);
        }

        public static Option Prompt<T>(string prompt, IEnumerable<T> values, Func<T, string> f)
        {
            int count=0;
            var options = values.Select(v => new Option()
            {
                Caption = f(v),
                Key = Options[count++],
                Data = v
            });
            return Prompt(prompt, options);
        }

        public static int? PromptInt(string prompt)
        {
            Console.Write(prompt);
            var str = "";
            while (true)
            {
                var k = Console.ReadKey();
                if (k.Key == ConsoleKey.Enter)
                    break;
                var c = k.KeyChar;
                if (IsControl(c)) continue;
                if (!IsDigit(c))
                {
                    if (c == 'x' || c == 'X') return null;
                    Console.Write('\b');
                    continue;
                }

                str += c;
            }

            if (str.IsNullOrEmpty()) return null;
            return int.TryParse(str, out var i) ? i : null;
        }

        public class Option
        {
            public char Key { get; set; }
            public string Caption { get; set; }
            public Action OnSelected { get; set; }
            public object Data { get; set; }
        }

        public static bool HasAnyFlag(params string[] flags) => flags.Any(HasFlag);
        public static bool HasFlag(string flag) => Environment.GetCommandLineArgs()
            .Any(a => new[] {flag, $"/{flag}", $"-{flag}", $"--{flag}"}.Any(f => f.EqualsOic(a)));
    }

    public class ConsoleObjectEditor
    {
        private Dictionary<string, Action<object>> _byName = new DictionaryOic<Action<object>>();
        private Dictionary<Type, Action<object>> _byType = new();

        public void EditObject<T>(T obj)
        {
            while (true)
            {
                var props = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public).ToList();
                var opts = new List<char>();
                var sb = new IndentingStringBuilder("  ");
                for (int i = 0; i < props.Count; i++)
                {
                    var c = ConsoleUtil.Options[i];
                    var p = props[i];
                    sb.AppendLine($"{c}) {p.Name} ({p.PropertyType})");
                }

                sb.AppendLine("x) Exit");
                opts.Add('x');
                var res = ConsoleUtil.Prompt(sb.ToString(), opts.ToArray());
                if (res.Equals('x'))
                    return;

                var prop = props[ConsoleUtil.Options.IndexOf(v => ToUpper(v).Equals(res))];
                if (prop.PropertyType.Name.StartsWith("List") && prop.PropertyType.IsGenericType)
                {

                }
            }
        }
        public void EditList<T>(List<T> lst)
        {
            Action Looped(Action<int> a) => () =>
            {

                while (true)
                {
                    var index = ConsoleUtil.PromptInt(
                        $"Please select an item index between 0 and {lst.Count - 1} (x to abort)");
                    if (null == index || index < 0)
                        break;
                    a(index.Value);
                }
            };
            var sb = new IndentingStringBuilder("  ");
            var constructor = typeof(T).GenerateConstructor<T>();

            var options = new List<ConsoleUtil.Option>()
            {
                {1, "List items", ()=>lst.Select((o, i) => $"{i}: {o}").ForEach(Console.WriteLine)},
                {constructor != null, 2, "Add item", ()=>lst.Add(constructor())},
                {3, "Edit item", Looped(i=>EditObject(lst[i]))},
                {4, "Remove item", Looped(lst.RemoveAt)},
                {'x', "Exit"}
            };
            
            while (true)
            {
                var res = ConsoleUtil.Prompt($"Edit {lst.GetType().SaneName()} with {lst.Count} items:", options);
                if (res.OnSelected != null)
                {
                    res.OnSelected();
                    continue;
                }
                if (res.Key == 'x') return;
            }
        }
    }

    public static class OptionExtensions
    {
        public static void Add(this List<ConsoleUtil.Option> lst, char key, string caption, Action? a)
        {
            a ??= () => { };
            lst.Add(new ConsoleUtil.Option() { Caption = caption, Key = key, OnSelected = a});
        }

        public static void Add(this List<ConsoleUtil.Option> lst, int key, string caption) =>
            lst.Add((char)key, caption, null);
        public static void Add(this List<ConsoleUtil.Option> lst, int key, string caption, Action a) =>
            lst.Add((char)key, caption, a);
        public static void Add(this List<ConsoleUtil.Option> lst, bool predicate, char key, string caption)
        {
            if (!predicate) return;
            lst.Add(key, caption);
        }
        public static void Add(this List<ConsoleUtil.Option> lst, bool predicate, int key, string caption) => lst.Add(predicate, (char)key, caption);
        public static void Add(this List<ConsoleUtil.Option> lst, bool predicate, int key, string caption, Action a)
        {
            if(!predicate) return;
            lst.Add(key, caption, a);
        }
    }


}
