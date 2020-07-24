using System.Collections.Generic;
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
            return converter.Read(ref reader, typeof(T), options);
        }

        public static IList<T> ReadArray<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            while (reader.TokenType != JsonTokenType.StartArray)
            {
                if (!reader.Read())
                {
                    throw new JsonException("Unexpected end of stream");
                }
            }
            var result = new List<T>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                result.Add(ReadValue<T>(ref reader, options));
            }
            return result;
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
    }
}
