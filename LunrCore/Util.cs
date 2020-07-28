using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lunr
{
    /// <summary>
    /// A bunch of helpers functions.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// A function to calculate the inverse document frequency for
        /// a posting.This is shared between the builder and the index.
        /// </summary>
        /// <param name="posting">The posting for a given term.</param>
        /// <param name="documentCount">The total number of documents.</param>
        /// <returns>The inverse document frequency.</returns>
        public static double InverseDocumentFrequency(InvertedIndexEntry posting, int documentCount)
        {
            int documentsWithTerm = 0;

            foreach ((string fieldName, FieldMatches value) in posting)
            {
                if (fieldName == "_index") continue; // Ignore the term index, its not a field
                documentsWithTerm += value.Count;
            }

            double x = (documentCount - documentsWithTerm + 0.5) / (documentsWithTerm + 0.5);

            return Math.Log(1 + Math.Abs(x));
        }

        /// <summary>
        /// An extension method that enables the deconstruction of dictionary entries.
        /// </summary>
        /// <example>
        /// ```cs
        /// foreach ((string key, Foo value) in someDictionaryOfFoos)
        /// {
        ///     // Do something with `key` and `value`...
        /// }
        /// ```
        /// </example>
        /// <param name="kvp">The key value pair to deconstruct.</param>
        /// <param name="key">The deconstructed key.</param>
        /// <param name="value">The deconstructed value.</param>
        internal static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        /// <summary>
        /// Formats a date like ECMA-269 specifies its ToString().
        /// </summary>
        /// <param name="dt">The DateTime to format.</param>
        /// <returns>The formatted string.</returns>
        internal static string ToEcmaString(this DateTime dt)
        {
            string timeZoneString = dt.ToString("zzz");
            return dt.ToString("ddd MMM dd yyyy HH:mm:ss") + " GMT" + timeZoneString.Substring(0, 3) + timeZoneString.Substring(4);
        }

        private static readonly Regex _separatorExpression = new Regex(@"[\s\-]+");

        internal static readonly Func<char, bool> IsLunrSeparatorFunc = IsLunrSeparator;

        /// <summary>
        /// Tests if a character is whitespace or a hyphen.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns>True if ch is whitespace or a hyphen.</returns>
        internal static bool IsLunrSeparator(this char ch)
            => _separatorExpression.IsMatch(ch.ToString());
    }
}
