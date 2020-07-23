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
