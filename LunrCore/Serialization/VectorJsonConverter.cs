using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr.Serialization
{
    internal class VectorJsonConverter : JsonConverter<Vector>
    {
        public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new Vector();

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("A vector can only be deserialized from an array.");
            }
            while (reader.AdvanceTo(JsonTokenType.Number, JsonTokenType.EndArray) != JsonTokenType.EndArray)
            {
                int index = reader.ReadValue<int>(options);
                double value = reader.ReadValue<double>(options);
                result.Insert(index, value);
            }
            reader.ReadOrThrow();
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (double component in value.Save())
            {
                writer.WriteNumberValue(component);
            }
            writer.WriteEndArray();
        }
    }
}