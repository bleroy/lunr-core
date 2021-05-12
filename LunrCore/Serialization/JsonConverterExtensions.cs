using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunr.Serialization
{
    /// <summary>
    /// A collection of useful helpers to write JSON converters.
    /// </summary>
    internal static class JsonConverterExtensions
    {
        private static readonly JsonTokenType[] _ignorableTokenTypes = new JsonTokenType[]
        {
            JsonTokenType.Comment,
            JsonTokenType.None
        };

        /// <summary>
        /// Gets the converter for a type from serializer options.
        /// </summary>
        /// <typeparam name="T">The type to get a converter for.</typeparam>
        /// <param name="options">The JSON serialization options.</param>
        /// <returns>The converter.</returns>
        public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options)
            => (JsonConverter<T>)options.GetConverter(typeof(T));

        /// <summary>
        /// Reads a value of the specified type from the reader, then moves to the next token.
        /// </summary>
        /// <typeparam name="T">The type to read.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="options">The JSON serialization options.</param>
        /// <returns>The value read from the reader.</returns>
        public static T ReadValue<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            JsonConverter<T> converter = options.GetConverter<T>();
            T result = converter.Read(ref reader, typeof(T), options)!;
            reader.ReadOrThrow();
            return result;
        }

        /// <summary>
        /// Reads on object from a reader. Arrays become `List&lt;object?&gt;`, and objects become `Dictionary&lt;string, object?&gt;`.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="options">The JSON serialization options.</param>
        /// <returns>The value read from the reader.</returns>
        public static object? ReadObject(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.AdvanceToToken();
            object? value = reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number => JsonSerializer.Deserialize<double>(ref reader, options),
                JsonTokenType.String => JsonSerializer.Deserialize<string>(ref reader, options),
                JsonTokenType.StartArray => reader.ReadArrayOfObjects(options),
                JsonTokenType.StartObject => reader.ReadDictionary(options),
                JsonTokenType.PropertyName => throw new JsonException("Unexpected property name."),
                JsonTokenType.EndArray => throw new JsonException("Unexpected end of array."),
                JsonTokenType.EndObject => throw new JsonException("Unexpected end of object."),
                _ => JsonSerializer.Deserialize(ref reader, typeof(object), options)
            };
            reader.ReadOrThrow();
            return value;
        }

        /// <summary>
        /// Reads a JSON array from the reader as a `List&lt;T&gt;`.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="options">The JSON serialization options.</param>
        /// <returns>The list read from the reader.</returns>
        public static IList<T> ReadArray<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.AdvanceTo(JsonTokenType.StartArray);
            var result = new List<T>();
            reader.ReadOrThrow();
            while (reader.AdvanceToToken() != JsonTokenType.EndArray)
            {
                result.Add(reader.ReadValue<T>(options));
            }
            return result;
        }

        /// <summary>
        /// Reads a JSON array as a `List&lt;object&gt;`, using the same conventions as `ReadObject`.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="options">The JSON serialization options.</param>
        /// <returns>The list read from the reader.</returns>
        public static IList<object?> ReadArrayOfObjects(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.AdvanceTo(JsonTokenType.StartArray);
            var result = new List<object?>();
            reader.ReadOrThrow();
            while (reader.AdvanceToToken() != JsonTokenType.EndArray)
            {
                result.Add(reader.ReadObject(options));
            }
            return result;
        }

        /// <summary>
        /// Reads a JSON array of arrays of key value pairs from a reader as a
        /// `Dictionary&lt;string, TValue&gt;` using the same conventions as `ReadObject`.
        /// </summary>
        /// <typeparam name="TValue">The type of the values</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="options">The JSON serialization options.</param>
        /// <returns>The dictionary read from the reader.</returns>
        public static Dictionary<string, TValue> ReadDictionaryFromKeyValueSequence<TValue>(
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

        /// <summary>
        /// Reads a JSON object from a reader as a `Dictionary&lt;string, object?&gt;` using the same conventions as `ReadObject`.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="options">The JSON serialization options.</param>
        /// <returns>The dictionary read from the reader.</returns>
        public static Dictionary<string, object?> ReadDictionary(
            this ref Utf8JsonReader reader,
            JsonSerializerOptions options)
        {
            var result = new Dictionary<string, object?>();
            reader.AdvanceTo(JsonTokenType.StartObject);
            reader.ReadOrThrow();
            while (reader.AdvanceTo(JsonTokenType.PropertyName, JsonTokenType.EndObject) != JsonTokenType.EndObject)
            {
                string propertyName = reader.ReadValue<string>(options);
                object? propertyValue = reader.ReadObject(options);
                result.Add(propertyName, propertyValue);
            }
            return result;
        }

        /// <summary>
        /// Advances the reader to the first token of one of the specified types, and returns the type effectively found first.
        /// The method will throw a `JsonException` if a non-ignored token (comment and none are ignored) is found before
        /// one of the specified token types or if the end of the document is reached.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="stopTokenTypes">The list of token types to stop on.</param>
        /// <returns>The token type that was found first.</returns>
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

        /// <summary>
        /// Advances the reader to the next non-ignored token.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The type of the next token.</returns>
        public static JsonTokenType AdvanceToToken(this ref Utf8JsonReader reader)
        {
            while (_ignorableTokenTypes.Contains(reader.TokenType))
            {
                reader.ReadOrThrow();
            }
            return reader.TokenType;
        }

        /// <summary>
        /// Advances the reader to the next token, or throws if the end of the document has been reached.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public static void ReadOrThrow(this ref Utf8JsonReader reader)
        {
            if (!reader.Read())
            {
                throw new JsonException("Unexpected end of stream");
            }
        }

        /// <summary>
        /// Writes an object property.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="writer">The writer.</param>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The value of the property to write.</param>
        /// <param name="options">The JSON serialization options.</param>
        public static void WriteProperty<T>(this Utf8JsonWriter writer, string propertyName, T value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value, options);
        }

        /// <summary>
        /// Writes a value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The JSON serialization options.</param>
        public static void WriteValue<T>(this Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonConverter<T> converter = options.GetConverter<T>();
            converter.Write(writer, value, options);
        }
    }
}
