using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr.Serialization
{
    internal sealed class IndexJsonConverter : JsonConverter<Index>
    {
        /// <summary>
        /// The lunr.js version that this version of the library is designed to be compatible with.
        /// </summary>
        private static readonly string _versionString = $"2.3.8";

        public override Index Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            InvertedIndex? invertedIndex = null;
            Dictionary<string, Vector>? fieldVectors = null;
            Pipeline? pipeline = null;
            IEnumerable<string>? fields = null;

            var tokenSetBuilder = new TokenSet.Builder();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("An index can only be deserialized from an object.");
            }
            reader.Read();
            while (reader.AdvanceTo(JsonTokenType.PropertyName, JsonTokenType.EndObject) != JsonTokenType.EndObject)
            {
                string propertyName = reader.ReadValue<string>(options);
                switch (propertyName)
                {
                    case "version":
                        string version = reader.ReadValue<string>(options);
                        if (version != _versionString)
                        {
                            System.Diagnostics.Debug.Write($"Version mismatch when loading serialised index. Current version of Lunr '{_versionString}' does not match serialized index '{version}'");
                        }
                        break;
                    case "invertedIndex":
                        invertedIndex = reader.ReadValue<InvertedIndex>(options);
                        break;
                    case "fieldVectors":
                        fieldVectors = reader.ReadDictionaryFromKeyValueSequence<Vector>(options);
                        break;
                    case "pipeline":
                        pipeline = new Pipeline(reader.ReadArray<string>(options));
                        break;
                    case "fields":
                        fields = reader.ReadArray<string>(options);
                        break;
                }
            }
            if (invertedIndex is null) throw new JsonException("Serialized index is missing invertedIndex.");
            if (fieldVectors is null) throw new JsonException("Serialized index is missing fieldVectors.");
            if (pipeline is null) throw new JsonException("Serialized index is missing a pipeline.");
            if (fields is null) throw new JsonException("Serialized index is missing a list of fields.");

            foreach (string term in invertedIndex.Keys)
            {
                tokenSetBuilder.Insert(term);
            }
            tokenSetBuilder.Finish();

            return new Index(invertedIndex, fieldVectors, tokenSetBuilder.Root, fields, pipeline);
        }

        public override void Write(Utf8JsonWriter writer, Index value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("version", _versionString);
            writer.WritePropertyName("fields");
            writer.WriteStartArray();
            foreach (string field in value.Fields)
            {
                writer.WriteStringValue(field);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("fieldVectors");
            writer.WriteStartArray();
            foreach ((string field, Vector vector) in value.FieldVectors)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(field);
                writer.WriteValue(vector, options);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteProperty("invertedIndex", value.InvertedIndex, options);
            writer.WritePropertyName("pipeline");
            writer.WriteStartArray();
            foreach (string fun in value.Pipeline.Save())
            {
                writer.WriteStringValue(fun);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}