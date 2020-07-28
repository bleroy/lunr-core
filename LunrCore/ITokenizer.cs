using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lunr
{
    public interface ITokenizer
    {
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
        IEnumerable<Token> Tokenize(
            object obj,
            TokenMetadata metadata,
            CultureInfo culture,
            Func<char, bool>? separator = null!);
    }
}