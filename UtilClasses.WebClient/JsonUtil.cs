using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UtilClasses.Files;
using UtilClasses.Json.Extensions;
using UtilClasses.Extensions.Objects;

namespace UtilClasses.WebClient;
public static class JsonUtil
{
    public static JsonSerializerSettings GetSettings()
    {
        var ret = new JsonSerializerSettings();
        ApplySettings(ret);
        return ret;
    }

    public enum Example
    {
        one,
        two
    }

    public static void ApplySettings(JsonSerializerSettings settings)
    {
        settings
            .WithBcc<object, Example, int>("Type", "Id", x => x
                .With<object>(Example.one)
                .With<object>(Example.two)
            )
            .WithBcc<object, Example>(x => x
                .With<object>(Example.one)
                .With<object>(Example.two)
            );

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

    public static bool SaveIfChanged(string path, object o) => FileSaver.SaveIfChanged(path, Serialize(o));
}