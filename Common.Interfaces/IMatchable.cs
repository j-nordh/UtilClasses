using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IMatchable<in T>
    {
        int GetMatchHash();
        bool Matches(T other);
    }
}
