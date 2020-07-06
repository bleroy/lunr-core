using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr
{
    internal class PostingJsonConverter : JsonConverter<Posting>
    {
        public override Posting Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("An inverted index can only be deserialized from an array.");
            }
            var result = new Posting();
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
                        (IDictionary<string, IDictionary<string, IList<object>>>)reader.ReadValue<Dictionary<string, Dictionary<string, List<object>>>>(options));
                }
                else throw new JsonException("Unexpected token.");
            }
            throw new JsonException("Unexpected end of stream.");
        }

        public override void Write(Utf8JsonWriter writer, Posting value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("_index", value.Index);
            foreach((string field, IDictionary<string, IDictionary<string, IList<object>>> val) in value)
            {
                writer.WriteProperty(field, val, options);
            }
            writer.WriteEndObject();
        }
    }
}