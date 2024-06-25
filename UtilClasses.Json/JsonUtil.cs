using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UtilClasses.Files;
using UtilClasses.Extensions.Objects;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UtilClasses.Cli;

namespace UtilClasses.Json;

public interface IJsonSettingSetter
{
    public void Apply(JsonSerializerSettings settings);
}
public static class JsonUtil
{
    public static ConsoleWriter Json(this ConsoleWriter wr, object o)
    {
        var json = Serialize(o);
        return wr.WriteLine(json);
    }
    public static List<IJsonSettingSetter> Settings { get; } = new List<IJsonSettingSetter>();
    public static JsonSerializerSettings GetSettings()
    {
        var ret = new JsonSerializerSettings();
        ApplySettings(ret);
        return ret;
    }
    static JsonUtil()
    {
        Settings.Default();
    }

    public static void ApplySettings(JsonSerializerSettings settings)
    {
        foreach (var s in Settings)
        {
            s.Apply(settings);
        }

        settings.Converters.Add(new StringEnumConverter());
        settings.Formatting = Formatting.Indented;
        settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
    }
    public static JsonSerializer GetSerializer() => JsonSerializer.Create(GetSettings());

    public static T Get<T>(string s) => JsonConvert.DeserializeObject<T>(s, GetSettings());
    public static bool TryGet<T>(string s, out T res)
    {
        res = default;
        try
        {
            res = JsonConvert.DeserializeObject<T>(s, GetSettings());
            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }

    public static T Load<T>(params string[] pathParts) => Get<T>(File.ReadAllText(Path.Combine(pathParts)));
    public static string Serialize(object o) => JsonConvert.SerializeObject(o, GetSettings());

    public static T Clone<T>(T obj) => Get<T>(Serialize(obj));

    public static bool SaveIfChanged(string path, object o) => FileSaver.SaveIfChanged(path, Serialize(o));
    public static List<IJsonSettingSetter> SetBcc<T, TKey>(
        Action<BaseClassConverterSetter<T, TKey>> config)
        where TKey : struct, Enum, IConvertible, IComparable
            => Settings.SetBcc(config);

    public static List<IJsonSettingSetter> SetBcc<T, TKey>(
        this List<IJsonSettingSetter> setters,
        Action<BaseClassConverterSetter<T, TKey>> config)
        where TKey : struct, Enum, IConvertible, IComparable
    {
        var conv = new BaseClassConverterSetter<T, TKey>();
        config(conv);
        //make sure we only use this BCC for this type and key
        setters.RemoveAll(s=>s is BaseClassConverterSetter<T, TKey>);
        setters.Add(conv);
        return setters;
    }
    public static List<IJsonSettingSetter> Default(this List<IJsonSettingSetter> setters)
    {
        setters.Clear();
        setters.Add(new DefaultSetter());
        return Settings;
    }
}