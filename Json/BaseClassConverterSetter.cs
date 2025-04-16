using System;
using Newtonsoft.Json;
using UtilClasses.Json.Extensions;

namespace UtilClasses.Json;

public class BaseClassConverterSetter<T, TKey> : IJsonSettingSetter where TKey : struct, Enum, IConvertible, IComparable
{
    private BaseClassConverter<T, TKey> _conv;
    public BaseClassConverterSetter()
    {
        _conv = new BaseClassConverter<T, TKey>();
    }
    public void Apply(JsonSerializerSettings settings)
    {
        settings.WithConverter(_conv);
    }
    public BaseClassConverterSetter<T, TKey> With<T2>(TKey k) where T2 : T, new()
    {
        _conv.With<T2>(k);
        return this;
    }
}
