using System;
using System.Collections.Generic;
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
                throw new JsonException("An inverted index entry can only be deserialized from an object.");
            }
            var result = new InvertedIndexEntry();
            reader.ReadOrThrow();
            while (reader.AdvanceTo(JsonTokenType.PropertyName, JsonTokenType.EndObject) != JsonTokenType.EndObject)
            {
                string fieldName = reader.ReadValue<string>(options);
                if (fieldName == "_index")
                {
                    result.Index = reader.ReadValue<int>(options);
                }
                else
                {
                    var fieldMatches = new FieldMatches();
                    reader.AdvanceTo(JsonTokenType.StartObject);
                    while (reader.AdvanceTo(JsonTokenType.PropertyName, JsonTokenType.EndObject) != JsonTokenType.EndObject)
                    {
                        string token = reader.ReadValue<string>(options);
                        var metadata = new FieldMatchMetadata();
                        reader.AdvanceTo(JsonTokenType.StartObject);
                        while (reader.AdvanceTo(JsonTokenType.PropertyName, JsonTokenType.EndObject) != JsonTokenType.EndObject)
                        {
                            string metadataName = reader.ReadValue<string>(options);
                            reader.AdvanceTo(JsonTokenType.StartArray);
                            reader.ReadOrThrow();
                            var data = new List<object?>();
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                // Special-case known metadata
                                if (metadataName == "position")
                                {
                                    data.Add(JsonSerializer.Deserialize<Slice>(ref reader, options));
                                }
                                else
                                {
                                    data.Add(reader.ReadObject(options));
                                }
                            }
                            reader.ReadOrThrow();
                            metadata.Add(metadataName, data);
                        }
                        reader.ReadOrThrow();
                        fieldMatches.Add(token, metadata);
                    }
                    reader.ReadOrThrow();
                    result.Add(fieldName, fieldMatches);
                }
            }
            reader.ReadOrThrow();
            return result;
        }

        public override void Write(Utf8JsonWriter writer, InvertedIndexEntry value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("_index", value.Index);
            foreach((string field, FieldMatches occurrences) in value)
            {
                writer.WritePropertyName(field);
                writer.WriteStartObject();
                foreach ((string doc, FieldMatchMetadata metadata) in occurrences)
                {
                    writer.WritePropertyName(doc);
                    writer.WriteStartObject();
                    foreach((string key, IList<object?> data) in metadata)
                    {
                        writer.WritePropertyName(key);
                        writer.WriteStartArray();
                        foreach (object? datum in data)
                        {
                            JsonSerializer.Serialize(writer, datum, options);
                        }
                        writer.WriteEndArray();
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
    }
}