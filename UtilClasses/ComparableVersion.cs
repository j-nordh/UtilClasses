using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Integers;
using UtilClasses.Extensions.Strings;

namespace UtilClasses
{
    class ComparableVersion:IComparable<ComparableVersion>
    {
        public ComparableVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public int CompareTo(ComparableVersion other) =>
            new List<Func<ComparableVersion, int>> {v => v.Major, v => v.Minor, v => v.Patch}
                .Select(f => f(this).CompareTo(f(other))).FirstOrDefault(res => res != 0);

        public static ComparableVersion Parse(string str)
        {
            var parts = str.Split('.').Select(s=>s.AsInt()).ToList();
            if(parts.Count!=3) throw new ArgumentException("The version is not formatted as x.y.z");
            return new ComparableVersion(parts[0], parts[1],parts[2]);
        }
    }
}
