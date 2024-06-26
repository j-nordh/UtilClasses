using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UtilClasses.Exceptions;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Types;

namespace UtilClasses
{
    public class Ensure
    {
        public static Ensure Argument { get; }
        public static Ensure That { get; }

        static Ensure()
        {
            Argument = new Ensure(true);
            That = new Ensure(false);
        }

        private Func<string, Exception> _fNull;
        private readonly string _location;
        private Func<string, string, Exception> _fEx;
        public Ensure(bool isArg)
        {
            _location = "";
            if (isArg)
            {
                _fNull = s => new ArgumentNullException(s);
                _fEx = (name, msg) => new ArgumentException(msg, name);
            }

            else
            {
                _fNull = _ => new NullReferenceException();
                _fEx = (_, msg) => new Exception(msg);
            }
        }

        public Ensure(Func<string, Exception> f, string location)
        {
            _fNull = f;
            _fEx = (_, msg) => f(msg);
            _location = location ?? "";
        }

        [ContractAnnotation("obj:null=>halt")]
        public void NotNull<TException>(object? obj, string message) where TException : Exception
        {
            if (null != obj) return;
            Throw<TException>(message);
        }

        [ContractAnnotation("obj:null=>halt")]
        public void NotNull(object? obj, string message)
        {
            if (null != obj) return;
            Throw(message);
        }


        public void NotNullOrEmpty(string s, string name)
        {
            if (!string.IsNullOrEmpty(s)) return;
            ThrowNull(name);
        }
        public void NotNullOrEmpty<T>(IEnumerable<T> items, string message)
        {
            if (items?.Any() ?? false) return;
            ThrowNull(message);
        }

        public void Is(bool b, string name, string message)
        {
            if (b) return;
            Throw(message, name);
        }
        [ContractAnnotation("message:null => halt")]
        public void Is(bool b, string message)
        {
            if (b) return;
            Throw(message);
        }

        public void Equals<T>(T a, T b, string message) where T : IComparable<T>
        {
            if(a.CompareTo(b) ==0) return;
            Throw((message));
        }

        public void Exists<T, TKey>(Dictionary<TKey, List<T>> dict, string kind, params TKey[] required) =>
            Exists(dict, kind, _location, required);
        public void Exists<T, TKey>(Dictionary<TKey, List<T>> dict, string kind, string location, params TKey[] required)
            => Exists(dict, lst => lst.Any(), kind, location, required);

        public void Exists<T, TKey>(Dictionary<TKey, T> dict, Func<T, bool> f, string kind, params TKey[] required) =>
            Exists(dict, f, kind, _location);
        public void Exists<T, TKey>(Dictionary<TKey, T> dict, Func<T, bool> f, string kind, string location, params TKey[] required)
        {
            var set = new HashSet<TKey>(required);
            foreach (var k in dict.Keys)
            {
                if (!f(dict[k])) continue;
                set.Remove(k);
            }
            if (!set.Any()) return;
            Missing(set, kind, location);
        }

        public void Exists<T, TKey, TReq>(Dictionary<TKey, T> dict, Func<T, TReq> f, string kind,params TReq[] required) 
            => Exists(dict, f, kind, required);
        public void Exists<T, TKey, TReq>(Dictionary<TKey, T> dict, Func<T, TReq> f, string kind, string location, params TReq[] required)
        {
            var set = new HashSet<TReq>(required);
            foreach (var k in dict.Keys)
            {
                set.Remove(f(dict[k]));
            }
            if (!set.Any()) return;
            Missing(set, kind, location);
            var msg = $"No {kind} for {set.Select(o => o?.ToString()??"").Join(", ")} found in {_location}.";
            Throw(msg);
            throw _fEx("", msg);
        }

        public void Exists<T, TReq>(List<T> lst, Func<T, TReq> f, string kind, params TReq[] required) =>
            Exists(lst, f, kind, _location, required);
        public void Exists<T, TReq>(List<T> lst, Func<T, TReq> f, string kind, string location, params TReq[] required)
        {
            var set = new HashSet<TReq>(required);
            foreach (var item in lst)
            {
                set.Remove(f(item));
            }

            if (!set.Any()) return;
            Missing(set, kind, location);
        }
        [ContractAnnotation("message:null => halt")]
        public void IsDir(string dir)
        {
            NotNull(dir, "Please provide a directory");
            if (Directory.Exists(dir)) return;
            throw new DirectoryNotFoundException($"The provided path is not a directory: {dir}");
        }
        [ContractAnnotation("message:null => halt")]
        public void IsFile(string file)
        {
            NotNull(file, "Please provide a directory");
            if (File.Exists(file)) return;
            throw new FileNotFoundException($"The provided path is not a file: {file}");
        }

        void Missing<T>(IEnumerable<T> keys, string kind, string location) => Missing(keys.Select(o => o?.ToString()??"").Join(", "), kind, location);
        void Missing(string ps, string kind, string location)
        {
            var msg = $"No {kind} for {ps} found in {location}. Terminating.";
            throw _fEx("", msg);
        }

        public void Throw(string msg)
        {
            msg = _location.IsNullOrEmpty()? msg:$"{_location}: {msg}";
            Throw(msg, "");
        }

        void Throw(string msg, string? arg)
        {
            msg = _location.IsNullOrEmpty() ? msg : $"{_location}: {msg}";
            throw _fEx(arg, msg);
        }
        void Throw<T>(string msg, string arg) where T:Exception
        {
            msg = _location.IsNullOrEmpty() ? msg : $"{_location}: {msg}";
            _fEx(arg, msg);
            var x = typeof(T).GenerateConstructor<string, T>()(msg);
            throw x;
        }

        void Throw<T>(string msg) where T : Exception => Throw(msg, null);
        void ThrowNull(string arg)
        {
            arg= _location.IsNullOrEmpty() ? arg: $"{_location}: {arg}";
            throw _fNull(arg);
        }
        
    }

}
