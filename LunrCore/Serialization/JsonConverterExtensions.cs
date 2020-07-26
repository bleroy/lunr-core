using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr.Serialization
{
    public static class JsonConverterExtensions
    {
        public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options)
            => (JsonConverter<T>)options.GetConverter(typeof(T));

        public static T ReadValue<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            JsonConverter<T> converter = options.GetConverter<T>();
            T result = converter.Read(ref reader, typeof(T), options);
            reader.ReadOrThrow();
            return result;
        }

        public static IList<T> ReadArray<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.AdvanceTo(JsonTokenType.StartArray);
            var result = new List<T>();
            reader.ReadOrThrow();
            while (reader.AdvanceToNextToken() != JsonTokenType.EndArray)
            {
                result.Add(reader.ReadValue<T>(options));
            }
            reader.ReadOrThrow();
            return result;
        }

        public static IDictionary<string, TValue> ReadDictionaryFromKeyValueSequence<TValue>(
            this ref Utf8JsonReader reader,
            JsonSerializerOptions options)
        {
            var result = new Dictionary<string, TValue>();
            reader.AdvanceTo(JsonTokenType.StartArray);
            reader.ReadOrThrow();
            while (reader.AdvanceTo(JsonTokenType.StartArray, JsonTokenType.EndArray) != JsonTokenType.EndArray)
            {
                reader.AdvanceTo(JsonTokenType.String);
                string propertyName = reader.ReadValue<string>(options);
                TValue propertyValue = reader.ReadValue<TValue>(options);
                result.Add(propertyName, propertyValue);
            }
            reader.ReadOrThrow();
            return result;
        }

        private static readonly JsonTokenType[] _ignorableTokenTypes = new JsonTokenType[]
        {
            JsonTokenType.Comment, JsonTokenType.None
        };

        public static JsonTokenType AdvanceTo(this ref Utf8JsonReader reader, params JsonTokenType[] stopTokenTypes)
        {
            while (!stopTokenTypes.Contains(reader.TokenType))
            {
                reader.ReadOrThrow();
                if (!stopTokenTypes.Contains(reader.TokenType) && !_ignorableTokenTypes.Contains(reader.TokenType))
                {
                    throw new JsonException($"Unexpected token {reader.TokenType}.");
                }
            }
            return reader.TokenType;
        }

        public static JsonTokenType AdvanceToNextToken(this ref Utf8JsonReader reader)
        {
            while (_ignorableTokenTypes.Contains(reader.TokenType))
            {
                reader.ReadOrThrow();
            }
            return reader.TokenType;
        }

        public static void WriteProperty<T>(this Utf8JsonWriter writer, string propertyName, T value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value, options);
        }

        public static void WriteValue<T>(this Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonConverter<T> converter = options.GetConverter<T>();
            converter.Write(writer, value, options);
        }

        public static void ReadOrThrow(this ref Utf8JsonReader reader)
        {
            if (!reader.Read())
            {
                throw new JsonException("Unexpected end of stream");
            }
        }
    }
}
