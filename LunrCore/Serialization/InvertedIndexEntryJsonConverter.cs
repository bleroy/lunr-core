using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr.Serialization
{
    internal class InvertedIndexEntryJsonConverter : JsonConverter<InvertedIndexEntry>
    {
        public override InvertedIndexEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("An inverted index can only be deserialized from an array.");
            }
            var result = new InvertedIndexEntry();
            reader.Read();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return result;
                }
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    result.Add(
                        propertyName,
                        reader.ReadValue<FieldOccurrences>(options));
                }
                else throw new JsonException("Unexpected token.");
            }
            throw new JsonException("Unexpected end of stream.");
        }

        public override void Write(Utf8JsonWriter writer, InvertedIndexEntry value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("_index", value.Index);
            foreach((string field, FieldOccurrences occurrences) in value)
            {
                writer.WritePropertyName(field);
                writer.WriteStartObject();
                foreach ((string doc, Metadata metadata) in occurrences)
                {
                    writer.WritePropertyName(doc);
                    writer.WriteStartObject();
                    foreach((string key, object data) in metadata)
                    {
                        writer.WriteProperty(key, data, options);
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
    }
}