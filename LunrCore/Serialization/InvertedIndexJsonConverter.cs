using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr.Serialization
{
    internal class InvertedIndexJsonConverter : JsonConverter<InvertedIndex>
    {
        public override InvertedIndex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("An inverted index can only be deserialized from an array.");
            }
            var serializedVectors = new List<(string term, InvertedIndexEntry posting)>();
            reader.ReadOrThrow();
            while (reader.AdvanceTo(JsonTokenType.StartArray, JsonTokenType.EndArray) != JsonTokenType.EndArray)
            {
                reader.AdvanceTo(JsonTokenType.String);
                serializedVectors.Add((
                    reader.ReadValue<string>(options),
                    reader.ReadValue<InvertedIndexEntry>(options)));
            }
            return new InvertedIndex(serializedVectors);
        }

        public override void Write(Utf8JsonWriter writer, InvertedIndex value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach((string term, InvertedIndexEntry entry) in value.OrderBy(kvp => kvp.Key))
            {
                writer.WriteStartArray();
                writer.WriteValue(term, options);
                writer.WriteValue(entry, options);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}