using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr
{
    internal class IndexJsonConverter : JsonConverter<Index>
    {
        public override Index Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            InvertedIndex? invertedIndex = null;
            IDictionary<string, Vector>? fieldVectors = null;
            Pipeline? pipeline = null;
            IEnumerable<Field>? fields = null;

            var tokenSetBuilder = new TokenSet.Builder();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("An index can only be deserialized from an object.");
            }
            reader.Read();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (invertedIndex is null) throw new JsonException("Serialized index is missing invertedIndex.");
                    if (fieldVectors is null) throw new JsonException("Serialized index is missing fieldVectors.");
                    if (pipeline is null) throw new JsonException("Serialized index is missing a pipeline.");
                    if (fields is null) throw new JsonException("Serialized index is missing a list of fields.");

                    return new Index(invertedIndex, fieldVectors, tokenSetBuilder.Root, fields, pipeline);
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    switch(propertyName)
                    {
                        case "version":
                            System.Diagnostics.Debug.Write($"Version mismatch when loading serialised index. Current version of Lunr '{typeof(Index).Assembly.GetName().Version}' does not match serialized index '{reader.ReadValue<string>(options)}'");
                            break;
                        case "invertedIndex":
                            invertedIndex = reader.ReadValue<InvertedIndex>(options);
                            break;
                        case "fieldVectors":
                            fieldVectors = reader.ReadValue<IDictionary<string, Vector>>(options);
                            break;
                        case "pipeline":
                            pipeline = new Pipeline(reader.ReadValue<IEnumerable<string>>(options));
                            break;
                        case "fields":
                            fields = reader.ReadValue<IEnumerable<Field>>(options);
                            break;
                    }
                }
            }
            throw new JsonException("Unexpectedly reached the end of the stream.");
        }

        public override void Write(Utf8JsonWriter writer, Index value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteProperty("version", typeof(Index).Assembly.GetName().Version, options);
            writer.WriteProperty("invertedIndex", value.InvertedIndex, options);
            writer.WriteProperty("fieldVectors", value.FieldVectors, options);
            writer.WriteProperty("pipeline", value.Pipeline, options);
            writer.WriteProperty("fields", value.Fields, options);
            writer.WriteEndObject();
        }
    }
}