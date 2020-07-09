using System;
using System.Collections.Generic;
using System.Linq;

namespace Lunr
{
    public class Token
    {
        /// <summary>
        /// Creates a new token from a string.
        /// </summary>
        /// <param name="tokenString">The token string.</param>
        /// <param name="metadata">Metadata associated with this token.</param>
        public Token(string tokenString, IDictionary<string, object>? metadata = null!)
        {
            String = tokenString ?? "";
            Metadata = metadata ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a new token from a string.
        /// </summary>
        /// <param name="tokenString">The token string.</param>
        /// <param name="metadata">Metadata associated with this token.</param>
        public Token(string tokenString, params (string key, object value)[] metadata)
            : this(tokenString, metadata.ToDictionary(kvp => kvp.key, kvp => kvp.value)) { }

        /// <summary>
        /// The string token being wrapped.
        /// </summary>
        public string String { get; private set; }

        /// <summary>
        /// Metadata associated with this token.
        /// </summary>
        public IDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Applies the given function to the wrapped string token.
        /// </summary>
        /// <param name="transformation">A transformation on the token string.</param>
        /// <returns>The same token (not a clone), but its string has been mutated.</returns>
        public Token Update(Func<string, string> transformation)
        {
            String = transformation(String);
            return this;
        }

        /// <summary>
        /// Applies the given function to the wrapped string token.
        /// </summary>
        /// <param name="transformation">A transformation on the token string.</param>
        /// <returns>The same token (not a clone), but its string has been mutated.</returns>
        public Token Update(Func<string, IDictionary<string, object>, string> transformation)
        {
            String = transformation(String, Metadata);
            return this;
        }

        /// <summary>
        /// Clones this token, optionally applying a transformation to the token string.
        /// </summary>
        /// <param name="transformation">An optional transformation to apply to the token string.</param>
        /// <returns>A clone of the token.</returns>
        public Token Clone(Func<string, string>? transformation = null)
            => new Token(transformation is null ? String : transformation(String), new Dictionary<string, object>(Metadata));

        public override string ToString() => String;

        public static implicit operator string(Token token) => token.String;
    }
}
