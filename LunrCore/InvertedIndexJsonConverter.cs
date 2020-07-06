using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr
{
    internal class InvertedIndexJsonConverter : JsonConverter<InvertedIndex>
    {
        public override InvertedIndex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("An inverted index can only be deserialized from an array.");
            }
            var serializedVectors = new List<(string term, Posting posting)>();
            reader.Read();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return new InvertedIndex(serializedVectors);
                }
                else if (reader.TokenType == JsonTokenType.StartArray)
                {
                    serializedVectors.Add((
                        reader.ReadValue<string>(options),
                        reader.ReadValue<Posting>(options)));
                    reader.Read();
                }
                else throw new JsonException("Unexpected token.");
            }
            throw new JsonException("Unexpected end of stream.");
        }

        public override void Write(Utf8JsonWriter writer, InvertedIndex value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach((string term, Posting posting) in value.OrderBy(kvp => kvp.Key))
            {
                writer.WriteStartArray();
                writer.WriteValue(term, options);
                writer.WriteValue(posting, options);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}