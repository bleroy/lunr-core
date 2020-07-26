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
                string propertyName = reader.ReadValue<string>(options);
                if (propertyName == "_index")
                {
                    result.Index = reader.ReadValue<int>(options);
                }
                else
                {
                    var occurrences = new FieldOccurrences();
                    reader.AdvanceTo(JsonTokenType.StartObject);
                    while (reader.AdvanceTo(JsonTokenType.PropertyName, JsonTokenType.EndObject) != JsonTokenType.EndObject)
                    {
                        string field = reader.ReadValue<string>(options);
                        var metadata = new Metadata();
                        reader.AdvanceTo(JsonTokenType.StartObject);
                        while (reader.AdvanceTo(JsonTokenType.PropertyName, JsonTokenType.EndObject) != JsonTokenType.EndObject)
                        {
                            string doc = reader.ReadValue<string>(options);
                            reader.AdvanceTo(JsonTokenType.StartArray);
                            reader.ReadOrThrow();
                            var data = new List<object>();
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                data.Add(JsonSerializer.Deserialize(ref reader, typeof(object), options));
                            }
                            reader.ReadOrThrow();
                            metadata.Add(doc, data);
                        }
                        reader.ReadOrThrow();
                        occurrences.Add(field, metadata);
                    }
                    reader.ReadOrThrow();
                    result.Add(propertyName, occurrences);
                }
            }
            reader.ReadOrThrow();
            return result;
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
                    foreach((string key, IList<object> data) in metadata)
                    {
                        writer.WritePropertyName(key);
                        writer.WriteStartArray();
                        foreach (object datum in data)
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