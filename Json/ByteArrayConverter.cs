using Newtonsoft.Json;
using System;
using UtilClasses.Core.Extensions.Bytes;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Json
{
    public class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToHexString(":"));
        }

        public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue,
            JsonSerializer serializer) =>
            reader.Value?.ToString().HexToByteArray();
    }
}
