using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lunr
{
    public sealed class Tokenizer : ITokenizer
    {
        /// <summary>
        /// Gets the default separator matching spaces and `-` used by the tokenizer.
        /// </summary>
        public static readonly Func<char, bool> DefaultSeparator = Util.IsLunrSeparatorFunc;
        
        /// <summary>
        /// A function for splitting a string into tokens ready to be inserted into
        /// the search index.
        ///
        /// This tokenizer will convert its parameter to a string by calling `ToString` and
        /// then will split this string on white space, punctuation and separators.
        /// </summary>
        /// <param name="obj">The object to tokenize.</param>
        /// <param name="metadata">Optional metadata can be passed to the tokenizer, this metadata will be cloned and
        /// added as metadata to every token that is created from the object to be tokenized.</param>
        /// <returns>The list of tokens extracted from the string.</returns>
        public IEnumerable<Token> Tokenize(
            object obj,
            TokenMetadata metadata,
            CultureInfo culture,
            Func<char, bool>? separator = null!)
        {
            if (obj is null)
            {
                yield break;
            }

            string str = obj switch
            {
                DateTime dt => dt.ToEcmaString(),
                _ => obj.ToString()
            };

            int tokenCount = 0;

            separator ??= DefaultSeparator;

            for (int sliceEnd = 0, sliceStart = 0; sliceEnd <= str.Length; sliceEnd++)
            {
                char ch = sliceEnd < str.Length ? str[sliceEnd] : char.MinValue;
                int sliceLength = sliceEnd - sliceStart;

                if (separator(ch) || sliceEnd == str.Length)
                {
                    if (sliceLength > 0)
                    {
                        TokenMetadata? tokenMetadata
                            = metadata is null ? new TokenMetadata() : new TokenMetadata(metadata);
                        tokenMetadata["position"] = new Slice(sliceStart, sliceLength);
                        tokenMetadata["index"] = tokenCount++;

                        yield return new Token(
                            str.Substring(sliceStart, sliceEnd - sliceStart).ToLower(culture ?? CultureInfo.CurrentCulture),
                            tokenMetadata);
                    }

                    sliceStart = sliceEnd + 1;
                }
            }
        }

        /// <summary>
        /// A function for splitting a string into tokens ready to be inserted into
        /// the search index.
        ///
        /// This tokenizer will convert its parameter to a string by calling `ToString` and
        /// then will split this string on white space, punctuation and separators.
        /// </summary>
        /// <param name="obj">The object to tokenize.</param>
        /// <returns>The list of tokens extracted from the string.</returns>
        public IEnumerable<Token> Tokenize(object obj)
            => Tokenize(obj, new TokenMetadata(), CultureInfo.CurrentCulture);

        /// <summary>
        /// A function for splitting a string into tokens ready to be inserted into
        /// the search index.
        ///
        /// This tokenizer will convert its parameter to a string by calling `ToString` and
        /// then will split this string on white space, punctuation and separators.
        /// </summary>
        /// <param name="enumerable">The list of objects to tokenize.</param>
        /// <param name="metadata">Optional metadata can be passed to the tokenizer, this metadata will be cloned and
        /// added as metadata to every token that is created from the object to be tokenized.</param>
        /// <returns>The list of tokens extracted from the string.</returns>
        public IEnumerable<Token> Tokenize(
            IEnumerable<object> enumerable,
            TokenMetadata metadata)
        {
            foreach (object single in enumerable)
            {
                yield return new Token(
                    single is null ? "" : single.ToString(),
                    metadata is null ? new TokenMetadata() : new TokenMetadata(metadata));
            }
        }

        /// <summary>
        /// A function for splitting a string into tokens ready to be inserted into
        /// the search index.
        ///
        /// This tokenizer will convert its parameter to a string by calling `ToString` and
        /// then will split this string on white space, punctuation and separators.
        /// </summary>
        /// <param name="enumerble">The list of strings to tokenize.</param>
        /// <param name="metadata">Optional metadata can be passed to the tokenizer, this metadata will be cloned and
        /// added as metadata to every token that is created from the object to be tokenized.</param>
        /// <returns>The list of tokens extracted from the string.</returns>
        public IEnumerable<Token> Tokenize(
            IEnumerable<string> enumerable)
        {
            foreach (string single in enumerable)
            {
                yield return new Token(
                    single ?? "",
                    new TokenMetadata());
            }
        }

        /// <summary>
        /// A function for splitting a string into tokens ready to be inserted into
        /// the search index.
        ///
        /// This tokenizer will convert its parameter to a string by calling `ToString` and
        /// then will split this string on white space, punctuation and separators.
        /// </summary>
        /// <param name="str">The string to tokenize.</param>
        /// <param name="metadata">Optional metadata can be passed to the tokenizer, this metadata will be cloned and
        /// added as metadata to every token that is created from the object to be tokenized.</param>
        /// <returns>The list of tokens extracted from the string.</returns>
        public IEnumerable<Token> Tokenize(
            string str,
            TokenMetadata metadata,
            CultureInfo culture)
            => Tokenize((object)str, metadata, culture);

        /// <summary>
        /// A function for splitting a string into tokens ready to be inserted into
        /// the search index.
        ///
        /// This tokenizer will convert its parameter to a string by calling `ToString` and
        /// then will split this string on white space, punctuation and separators.
        /// </summary>
        /// <param name="str">The string to tokenize.</param>
        /// <param name="metadata">Optional metadata can be passed to the tokenizer, this metadata will be cloned and
        /// added as metadata to every token that is created from the object to be tokenized.</param>
        /// <returns>The list of tokens extracted from the string.</returns>
        public IEnumerable<Token> Tokenize(
            string str,
            TokenMetadata metadata)
            => Tokenize((object)str, metadata, CultureInfo.CurrentCulture);

        /// <summary>
        /// A function for splitting a string into tokens ready to be inserted into
        /// the search index.
        ///
        /// This tokenizer will convert its parameter to a string by calling `ToString` and
        /// then will split this string on white space, punctuation and separators.
        /// </summary>
        /// <param name="str">The string to tokenize.</param>
        /// <returns>The list of tokens extracted from the string.</returns>
        public IEnumerable<Token> Tokenize(
            string str,
            CultureInfo culture)
            => Tokenize(str, new TokenMetadata(), culture);

        /// <summary>
        /// A function for splitting a string into tokens ready to be inserted into
        /// the search index.
        ///
        /// This tokenizer will convert its parameter to a string by calling `ToString` and
        /// then will split this string on white space, punctuation and separators.
        /// </summary>
        /// <param name="str">The string to tokenize.</param>
        /// <returns>The list of tokens extracted from the string.</returns>
        public IEnumerable<Token> Tokenize(string str)
            => Tokenize(str, CultureInfo.CurrentCulture);
    }
}
