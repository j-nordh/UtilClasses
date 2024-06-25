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
        public static List<string> CompanyTypes { get; }
        public static List<string> RoadNames { get; }
        public static List<string> Cities { get; }
        public static List<string> Countries { get; }
        public static List<(string Name, decimal Latitude, decimal Longitude)> Places { get; }
        private readonly List<string> _nouns;
        private readonly List<string> _adjectives;
        private readonly List<string> _firstNames;
        private readonly List<string> _lastNames;
        private readonly List<(string Name, decimal Latitude, decimal Longitude)> _places;
        public UnsafeGenerator Unsafe { get; }

        static IdGenerator()
        {
            var ass = typeof(IdGenerator).Assembly;

            List<string> LoadList(string name) =>
                ass.GetResourceString(name).SplitLines().ToList();

            Nouns = LoadList("Nouns.txt");
            Adjectives = LoadList("Adjectives.txt");
            FirstNames = LoadList("FirstNames.txt");
            LastNames = LoadList("LastNames.txt");
            CompanyTypes = LoadList("CompanyTypes.txt");
            RoadNames = LoadList("RoadNames.txt");
            Cities = LoadList("Cities.txt");
            Countries = LoadList("Countries.txt");

            Places = ass.GetResourceString("Places.txt")
                .SplitLines()
                .Select(l => l.SplitREE(";"))
                .Where(ps => ps.Length == 3)
                .Select(ps => (ps[0], ps[2].AsDecimal(), ps[1].AsDecimal()))
                .ToList();
        }

        public class UnsafeGenerator
        {
            private readonly Random _rnd;

            public UnsafeGenerator()
            {
                _rnd = new();
            }

            public UnsafeGenerator(int seed)
            {
                _rnd = new Random(seed);
            }

            public string Thing() =>
                $"{Get(Adjectives)} {Get(Nouns)}";

            public string Adjective() => Get(Adjectives);
            public string Noun() => Get(Nouns);
            public string Name() =>
                $"{Get(FirstNames)} {Get(LastNames)}";

            public string CompanyName()
                => $"{Get(Adjectives)} {Get(Nouns)} {Get(CompanyTypes)}";

            public ICompany Company() => new CompanyImpl(this);
            public string Address() => $"{Get(RoadNames)} {_rnd.Next(1, 100)}";
            public string Zipcode() => $"{_rnd.Next(10000, 99999)}";
            public string Place() => $"{_rnd.Next(10000, 99999)}";
            public string City() =>  Get(Cities);
            public string Country() => Get(Countries);

            private string Get(List<string> ls) => ls[_rnd.Next(0, ls.Count)];

            public interface ICompany
            {
                public string Name { get; }
                public string WebSite { get; }
                public string Email { get; }
                public string Owner { get; }
                public string Address { get; }
                public string City { get; }
            }

            private class CompanyImpl : ICompany
            {
                public string Name { get; }
                public string WebSite { get; }
                public string Email { get; }
                public string Owner { get; }
                public string Address { get; }
                public string City { get; }

                public CompanyImpl(UnsafeGenerator gen)
                {
                    Name = gen.CompanyName();
                    Owner = gen.Name();
                    var domain = Name.Split(' ').Take(2).Join("").ToLower() + ".com";
                    WebSite = $"www.{domain}";
                    Email = $"info@{domain}";
                    Address = $"{gen.Address()}\n{gen.Zipcode()}";
                    City = gen.City();
                    
                }
            }
        }

        public IdGenerator(bool shuffle = false)
        {
            _nouns = Nouns.ToList();
            _adjectives = Adjectives.ToList();
            _places = Places.ToList();
            _firstNames = FirstNames.ToList();
            _lastNames = LastNames.ToList();
            Unsafe = new UnsafeGenerator();
            if (!shuffle) return;

            _nouns.Shuffle();
            _adjectives.Shuffle();
            _places.Shuffle();
            _firstNames.Shuffle();
            _lastNames.Shuffle();
        }


        #region Data

        public string Name() => Get(_firstNames, _lastNames, (a, b) => $"{a} {b}");


        public string Thing() => Get(_adjectives, _nouns, (a, b) => $"{a} {b}");


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

        private string Get(List<string> lstA, List<string> lstB, Func<string, string, string>? f = null) =>
            Get(lstA, lstB, WithLoop(f ?? ((a, b) => $"{a} {b}")));

        private string Get(List<string> lstA, List<string> lstB, Func<string, string, int, string> f)
        {
            var i = (int)(_count++ % int.MaxValue);
            var a = lstA[i % lstA.Count];
            var b = lstB[i / lstA.Count % lstB.Count];
            var loopCount = (int)(_count / (ulong)(lstA.Count * lstB.Count));
            return f(a, b, loopCount);
        }

        private string Get(List<string> lstA, List<string> lstB, List<string> lstC,
            Func<string, string, string, int, string> f)
        {
            var i = (int)(_count++ % int.MaxValue);
            var a = lstA[i % lstA.Count];
            var b = lstB[i / lstA.Count % lstB.Count];
            var c = lstB[i / (lstA.Count * lstB.Count) % lstC.Count];
            var loopCount = (int)(_count / (ulong)(lstA.Count * lstB.Count * lstC.Count));
            return f(a, b, c, loopCount);
        }

        #endregion
    }
}