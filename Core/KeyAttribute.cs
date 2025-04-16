using System;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core;

public class KeyAttribute : Attribute, IStringMatchable
{
    public string Key { get; }
    public KeyAttribute(string key)
    {
        Key = key;
    }
    public bool Matches(string s) => Key.EqualsOic(s);
}