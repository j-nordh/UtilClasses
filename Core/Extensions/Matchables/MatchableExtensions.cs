using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Core.Extensions.Dictionaries;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Types;
using UtilClasses.Interfaces;

namespace UtilClasses.Core.Extensions.Matchables;

public static class MatchableExtensions
{
    public static MatchResult<T> MatchWith<T>(this IEnumerable<T> first, IEnumerable<T> second) where T: class, IMatchable<T>
    {
        var ret = new MatchResult<T>();
        var lstA = new List<T>();
        var lstB = new List<T>();
        if (typeof(T).CanBe<IHasLongId>())
        {
            ret = first.Cast<IHasLongId>().MatchWithId(second.Cast<IHasLongId>()).Cast<T>();
            lstA = ret.UnmatchedFirst.ToList();
            lstB = ret.UnmatchedSecond.ToList();
            ret.UnmatchedFirst.Clear();
            ret.UnmatchedSecond.Clear();
        }
        else
        {
            lstA = first.ToList();
            lstB = second.ToList();
        }
        var matchDict  = new Dictionary<int, List<T>>();
        foreach (var b in lstB)
        {
            matchDict.GetOrAdd(b.GetMatchHash()).Add(b);
        }

        foreach (var a in lstA)
        {
            var mh = a.GetMatchHash();
            var bucket = matchDict.Maybe(mh);
            var b = bucket?.FirstOrDefault(a.Matches);
            if (null == b)
            {
                ret.UnmatchedFirst.Add(a);
                continue;
            }

            bucket?.Remove(b);
            ret.Add(a,b);
        }

        return ret;
    }

    public static MatchResult<T> MatchWithId<T>(this IEnumerable<T> first, IEnumerable<T> second) where T : class, IHasLongId
    {
        var ret = new MatchResult<T>();
        var dict = new Dictionary<long, T>();
        foreach (var b in second)
        {
            if(null ==b) continue;
            if (b.Id > 0) {dict[b.Id] = b; continue;}
            ret.UnmatchedSecond.Add(b);
        }
        foreach (var a in first.NotNull().Where(a => a.Id > 0))
        {
            var b = dict.Maybe(a.Id);
            if (null == b)
            {
                ret.UnmatchedFirst.Add(a);
                continue;
            }
            ret.Pairs.Add((a, b));
            dict.Remove(a.Id);
        }

        return ret;
    }

    public class MatchResult<T>
    {
        public List<(T,T)> Pairs { get; private set; }
        public List<T> UnmatchedFirst { get; private set; }
        public List<T> UnmatchedSecond { get; private set; }

        public MatchResult()
        {
            Pairs = new List<(T, T)>();
            UnmatchedFirst= new List<T>();
            UnmatchedSecond = new List<T>();
        }

        public MatchResult<T2> Cast<T2>()  where T2:class
        {
            return new MatchResult<T2>()
            {
                Pairs = Pairs.Select(p=>(p.Item1 as T2, p.Item2 as T2)).ToList(),
                UnmatchedFirst = UnmatchedFirst.Cast<T2>().ToList(),
                UnmatchedSecond = UnmatchedSecond.Cast<T2>().ToList()
            };
        }

        public void Add(T a, T b) => Pairs.Add((a, b));
        public IEnumerable<T> Merge(Func<T, T, T> merger) => Pairs.Select(p => merger(p.Item1, p.Item2));
        public bool Perfect => !UnmatchedFirst.Any() && !UnmatchedSecond.Any();
    }
}