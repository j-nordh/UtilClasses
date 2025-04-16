using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Json
{
    public class IpAddressConverter : JsonConverter<IPAddress>
    {
        public override void WriteJson(JsonWriter writer, IPAddress value, JsonSerializer serializer) =>
            JToken.FromObject(value.ToString()).WriteTo(writer);

        public override IPAddress ReadJson(JsonReader reader, Type objectType, IPAddress existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var tok = JToken.Load(reader).ToString();
            return tok.IsNullOrWhitespace() ? null : IPAddress.Parse(tok);
        }
    }
}