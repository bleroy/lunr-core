using System;

namespace Lunr
{
    public sealed class Token
    {
        /// <summary>
        /// Creates a new token from a string.
        /// </summary>
        /// <param name="tokenString">The token string.</param>
        /// <param name="metadata">Metadata associated with this token.</param>
        public Token(string tokenString, TokenMetadata? metadata = null)
        {
            String = tokenString ?? "";
            Metadata = metadata ?? new TokenMetadata();
        }

        /// <summary>
        /// Creates a new token from a string.
        /// </summary>
        /// <param name="tokenString">The token string.</param>
        /// <param name="metadata">Metadata associated with this token.</param>
        public Token(string tokenString, params (string key, object? value)[] metadata)
            : this(tokenString, new TokenMetadata(metadata)) { }

        /// <summary>
        /// The string token being wrapped.
        /// </summary>
        public string String { get; private set; }

        /// <summary>
        /// Metadata associated with this token.
        /// </summary>
        public TokenMetadata Metadata { get; }

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
        public Token Update(Func<string, TokenMetadata, string> transformation)
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
            => new Token(transformation is null ? String : transformation(String), new TokenMetadata(Metadata));

        public override string ToString() => String;

        public static implicit operator string(Token token) => token.String;

        public override bool Equals(object? obj)
            => obj switch
            {
                null => false,
                Token t => String.Equals(t.String),
                _ => false
            };

        public override int GetHashCode() => String.GetHashCode();
    }
}
