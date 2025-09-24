using System;
using System.Collections;
using System.Linq;

namespace UtilClasses.Core;

public static class ComparerHelper
{
    public static int Compare<T1>(T1 obj1, T1 obj2, params Func<T1, IComparable>[] selectors) => Compare(obj1,obj2, selectors.Select(f=>(f, f)).ToArray());
    public static int Compare<T1, T2>(T1 obj1, T2 obj2, params (Func<T1, IComparable> One, Func<T2, IComparable> Two)[] selectors)
    {
        if (obj1 is null || obj2 is null)
            return 1;
        if (ReferenceEquals(obj1, obj2)) return 0;
        return selectors.Select(pair => pair.One.Invoke(obj1).CompareTo(pair.Two.Invoke(obj2))).FirstOrDefault(res => res != 0);
    }
}