using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr.Serialization
{
    public sealed class SliceConverter : JsonConverter<Slice>
    {
        public override Slice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("A slice can only be deserialized from an array containing two integers.");
            }
            reader.AdvanceTo(JsonTokenType.Number);

            int start = reader.ReadValue<int>(options);
            int length = reader.ReadValue<int>(options);

            return new Slice(start, length);
        }

        public override void Write(Utf8JsonWriter writer, Slice value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.Start, options);
            writer.WriteValue(value.Length, options);
            writer.WriteEndArray();
        }
    }
}
