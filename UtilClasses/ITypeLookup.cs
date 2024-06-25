using System;
using System.Collections.Generic;

namespace UtilClasses;

public interface ITypeLookup
{
    IEnumerable<string> Names { get; }
    Type? Lookup(string key);
    IEnumerable<Type> Types();
    Type? this [string name] { get; }
    TId Id<TObj, TId>(TObj obj);
}