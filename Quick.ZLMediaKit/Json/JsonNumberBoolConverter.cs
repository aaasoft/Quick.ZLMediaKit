using System.Text.Json;
using System.Text;
using System;
using System.Text.Json.Serialization;

namespace Quick.ZLMediaKit.Json
{
    public class JsonNumberBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString() == "1";
                case JsonTokenType.Number:
                    return reader.GetInt32() == 1;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                default:
                    throw new FormatException($"值[{Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}]无法转换为bool?");
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}