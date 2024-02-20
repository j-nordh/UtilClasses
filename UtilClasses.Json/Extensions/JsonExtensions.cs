using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace UtilClasses.Json.Extensions
{
    public static class JsonExtensions
    {
        public static JsonSerializerSettings WithConverter<T>(this JsonSerializerSettings s, T conv) where T:JsonConverter
        {
            s.Converters.Add(conv);
            return s;
        }

        public static JsonSerializerSettings WithBcc<T, TKey>(this JsonSerializerSettings s,
            Action<BaseClassConverter<T, TKey>> setup) where TKey : struct, Enum, IConvertible, IComparable
        {
            var conv = new BaseClassConverter<T, TKey>();
            setup(conv);
            s.WithConverter(conv);
            return s;
        }
        public static JsonSerializerSettings WithBcc<T, TKey>(this JsonSerializerSettings s, string key,
            Action<BaseClassConverter<T, TKey>> setup) where TKey : struct, Enum, IConvertible, IComparable
        {
            var conv = new BaseClassConverter<T, TKey>(key);
            setup(conv);
            s.WithConverter(conv);
            return s;
        }
        public static JsonSerializerSettings WithBcc<T, TKey, TParam>(this JsonSerializerSettings s, string key, string paramKey,
            Action<BaseClassConverter<T, TKey, TParam>> setup) where TKey : struct, Enum, IConvertible, IComparable where TParam:struct, IConvertible
        {
            var conv = new BaseClassConverter<T, TKey, TParam>(key, paramKey);
            setup(conv);
            s.WithConverter(conv);
            return s;
        }
        public static JsonSerializerSettings WithBccId<T, TKey, TParam>(this JsonSerializerSettings s, 
            Action<BaseClassConverter<T, TKey, TParam>> setup) where TKey : struct, Enum, IConvertible, IComparable where TParam:struct, IConvertible
        {
            var conv = new BaseClassConverter<T, TKey, TParam>("Type", "Id");
            setup(conv);
            s.WithConverter(conv);
            return s;
        }
    }
}
