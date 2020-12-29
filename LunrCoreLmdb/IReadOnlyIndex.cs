using System.Collections.Generic;
using Lunr;

namespace LunrCoreLmdb
{
    public interface IReadOnlyIndex
    {
        /// <summary>
        /// An index of term/field to document reference.
        /// </summary>
        InvertedIndexEntry? GetInvertedIndexEntryByKey(string key);

        /// <summary>
        /// Field vectors.
        /// </summary>
        IEnumerable<string> GetFieldVectorKeys();

        /// <summary>
        /// Field vectors.
        /// </summary>
        Vector? GetFieldVectorByKey(string key);

        /// <summary>
        /// A set of all corpus tokens.
        /// </summary>
        TokenSet IntersectTokenSets(TokenSet other);

        /// <summary>
        /// The names of indexed document fields.
        /// </summary>
        IEnumerable<string> GetFields();
    }
}
