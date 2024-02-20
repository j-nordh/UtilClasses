using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Cache;
using UtilClasses.Extensions.Assemblies;
using UtilClasses.Extensions.Decimals;
using UtilClasses.Extensions.Lists;
using UtilClasses.Extensions.Strings;

namespace UtilClasses
{
    public class IdGenerator
    {
        private ulong _count = 1;
        public long Long() => (long)_count++;
        private Random _rnd = new();

        public static List<string> Nouns { get; }
        public static List<string> Adjectives { get; }
        public static List<string> FirstNames { get; }
        public static List<string> LastNames { get; }
        public static List<(string Name, decimal Latitude, decimal Longitude)> Places { get; }
        private readonly List<string> _nouns;
        private readonly List<string> _adjectives;
        private readonly List<string> _firstNames;
        private readonly List<string> _lastNames;
        private readonly List<(string Name, decimal Latitude, decimal Longitude)> _places;
        
        static IdGenerator()
        {
            var ass = typeof(IdGenerator).Assembly;
            List<string> LoadList(string name) =>
                ass.GetResourceString(name).SplitLines().ToList();

            Nouns = LoadList("Nouns.txt");
            Adjectives = LoadList("Adjectives.txt");
            FirstNames = LoadList("FirstNames.txt");
            LastNames = LoadList("LastNames.txt");
            
            Places = ass.GetResourceString("Places.txt")
                .SplitLines()
                .Select(l => l.SplitREE(";"))
                .Where(ps => ps.Length == 3)
                .Select(ps => (ps[0], ps[2].AsDecimal(), ps[1].AsDecimal()))
                .ToList();
        }


        public IdGenerator(bool shuffle = false)
        {
            _nouns = Nouns.ToList();
            _adjectives = Adjectives.ToList();
            _places = Places.ToList();
            _firstNames = FirstNames.ToList();
            _lastNames = LastNames.ToList();
            if (!shuffle) return;

            _nouns.Shuffle();
            _adjectives.Shuffle();
            _places.Shuffle();
            _firstNames.Shuffle();
            _lastNames.Shuffle();
        }
        #region Data

        public string Name() => Get(_firstNames, _lastNames, (a, b) => $"{a} {b}");
        public string Name_unsafe() => $"{_firstNames[_rnd.Next(0, _firstNames.Count)]} {_lastNames[_rnd.Next(0, _lastNames.Count)]}";
        public string Thing() => Get(_adjectives, _nouns, (a, b) => $"{a} {b}");
        public string Thing_unsafe() => $"{_adjectives[_rnd.Next(0, _adjectives.Count)]} {_nouns[_rnd.Next(0, _nouns.Count)]}";

        public (string Name, decimal Latitude, decimal Longitude) Place() =>
            Get(_places, (p, l) => ($"{p.Name}{WithLoop(l)}", p.Latitude, p.Longitude));

        public string WithLoop(int l) => l == 0 ? "" : $"_{l}";
        public Func<TA, TB, int, string> WithLoop<TA, TB>(Func<TA, TB, string> f) =>
            (a, b, l) => $"{f(a, b)}{WithLoop(l)}";

        private string Get(List<string> lst) => Get(lst, (s, loopCount) =>
         {
             var loop = loopCount == 0 ? "" : $"_{loopCount}";
             return $"{s}{loop}";
         });

        private TOut Get<TIn, TOut>(List<TIn> lst, Func<TIn, int, TOut> f)
        {
            var i = (int)_count++;
            var loopCount = i / lst.Count;
            i = i % lst.Count;
            return f(lst[i], loopCount);
        }

        private string Get(List<string> lstA, List<string> lstB, Func<string, string, string> f) =>
            Get(lstA, lstB, WithLoop(f));
        private TOut Get<TInA, TInB, TOut>(List<TInA> lstA, List<TInB> lstB, Func<TInA, TInB, TOut> f) =>
            Get(lstA, lstB, (a, b, _) => f(a, b));

        private TOut Get<TInA, TInB, TOut>(List<TInA> lstA, List<TInB> lstB, Func<TInA, TInB, int, TOut> f)
        {
            var i = (int)_count++;
            var a = lstA[i % lstA.Count];
            var b = lstB[i / lstA.Count % lstB.Count];
            var loopCount = i / (lstA.Count * lstB.Count);
            return f(a, b, loopCount);
        }
        #endregion
    }
}
