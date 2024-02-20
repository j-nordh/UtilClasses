using System;
using UtilClasses.Extensions.Strings;
namespace UtilClasses
{
    public class KeyAttribute : Attribute, IStringMatchable
    {
        public string Key { get; }
        public KeyAttribute(string key)
        {
            Key = key;
        }
        public bool Matches(string s) => Key.EqualsOic(s);
    }


}
