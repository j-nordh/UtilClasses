using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UtilClasses.Core.Extensions.Enums;
using UtilClasses.Core.Extensions.Objects;
using UtilClasses.Core.Extensions.Strings;
using UtilClasses.Core.Extensions.Types;

namespace UtilClasses.Json
{
    public class BaseClassConverter<T, TKey> : CustomCreationConverter<T> where TKey : struct,Enum, IConvertible, IComparable
    {
        private readonly string _key;

        private TKey _currentKey;
        private Dictionary<TKey, Func<T>> _creators = new();

        public BaseClassConverter() : this("Type"){}
        public BaseClassConverter(string key)
        {
            _key = key;
        }
        public BaseClassConverter(string key, Dictionary<TKey, Func<T>> creators)
        {
            _key = key;
            _creators = creators;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JToken.ReadFrom(reader);
            if (jobj is JValue {Value: null})
                return null;
            _currentKey = Enum<TKey>.Parse(jobj[_key] ?? jobj[_key.MakeIt().CamelCase()]);
            return base.ReadJson(jobj.CreateReader(), objectType, existingValue, serializer);
        }

        public override T Create(Type objectType)
        {
            _creators.TryGetValue(_currentKey, out var ret);
            if (null == ret)
                throw new ArgumentOutOfRangeException(nameof(objectType),
                    $"The object type {objectType.SaneName()} could not be created. Has it been added to the BaseClassConverter?");
            return ret();
        }

        public BaseClassConverter<T, TKey> With(TKey key, Func<T> f)
        {
            _creators[key] = f;
            return this;
        }

        public BaseClassConverter<T, TKey> With<TTarget>(TKey key) where TTarget : T, new() =>
            With(key, () => new TTarget());
    }

    public class BaseClassConverter<T, TKey, TParam> : CustomCreationConverter<T>
        where TKey : struct, Enum, IConvertible, IComparable
        where TParam:struct, IConvertible
    {
        private readonly string _key;
        private readonly string _paramKey;
        private TParam _p;

        private TKey _currentKey;
        private Dictionary<TKey, Func<TParam, T>> _creators = new();

        public BaseClassConverter(string key, string paramKey)
        {
            _key = key;
            _paramKey = paramKey;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JToken.ReadFrom(reader);
            _currentKey = Enum<TKey>.Parse(jobj[_key]);
            _p = jobj[_paramKey].Convert<TParam>();
            return base.ReadJson(jobj.CreateReader(), objectType, existingValue, serializer);
        }
        public override T Create(Type objectType) => _creators[_currentKey](_p);
        public BaseClassConverter<T, TKey, TParam> With(TKey key, Func<TParam, T> f)
        {
            _creators[key] = f;
            return this;
        }

        public BaseClassConverter<T, TKey, TParam> With<TTarget>(TKey key) where TTarget : T
        {
            var f = typeof(TTarget).GenerateConstructor<TParam, T>();
            return With(key, f);
        }
    }
}
