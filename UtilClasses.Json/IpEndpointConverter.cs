using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Json
{
    public class IpEndpointConverter : JsonConverter<IPEndPoint>
    {
        public override void WriteJson(JsonWriter writer, IPEndPoint value, JsonSerializer serializer) =>
            JToken.FromObject(value.ToString()).WriteTo(writer);

        public override IPEndPoint ReadJson(JsonReader reader, Type objectType, IPEndPoint existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var tok = JToken.Load(reader).ToString();
            if (tok.IsNullOrWhitespace()) return null;
            if (!tok.ContainsOic(":")) throw new FormatException("The string must be on the format a.b.c.d:e where a-d are integers between 0 and 255 and e is an integer between 0 and 64k");
            var bits = tok.Split(':');
            
            return new IPEndPoint(IPAddress.Parse(bits[0]), int.Parse(bits[1]));
        }
    }
}
