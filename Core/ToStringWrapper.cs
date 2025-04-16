using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilClasses.Core;

public class ToStringWrapper
{
    public static ToStringWrapper<T> Create<T>(T value, Func<T, string> toString) => new ToStringWrapper<T>(value, toString);
    public static IEnumerable<ToStringWrapper<T>> Create<T>(IEnumerable<T> values, Func<T, string> toString) => values.Select(v => new ToStringWrapper<T>(v, toString));
    public static Func<T, ToStringWrapper<T>> Create<T>(Func<T, string> toString) => s => new ToStringWrapper<T>(s, toString);
}
public class ToStringWrapper<T>
{
    private Func<T, string> _toString;
    public ToStringWrapper(T value, Func<T, string> toString)
    {
        Value = value;
        _toString = toString;
    }

    public override string ToString() => _toString(Value);

    public T Value { get; }
}