using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UtilClasses.Json;

public class DefaultSetter : IJsonSettingSetter
{
    void IJsonSettingSetter.Apply(JsonSerializerSettings settings)
    {
        settings.Converters.Add(new StringEnumConverter());
        settings.Formatting = Formatting.Indented;
        settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
    }
}
