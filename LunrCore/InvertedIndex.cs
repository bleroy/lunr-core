using Lunr.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Lunr
{
    /// <summary>
    /// Inverted index.
    /// term -> field -> document -> metadataKey -> metadataValue[]
    /// </summary>
    [JsonConverter(typeof(InvertedIndexJsonConverter))]
    public sealed class InvertedIndex : Dictionary<string, InvertedIndexEntry>
    {
        public InvertedIndex()
        { }

        public InvertedIndex(IEnumerable<(string term, InvertedIndexEntry entry)> entries)
            : base(entries.ToDictionary(e => e.term, e => e.entry)) { }
    }
}
