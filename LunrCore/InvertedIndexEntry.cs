using Lunr.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Lunr
{
    /// <summary>
    /// Inverted index entry.
    /// field -> document -> metadataKey -> metadataValue[]
    /// </summary>
    [JsonConverter(typeof(InvertedIndexEntryJsonConverter))]
    public sealed class InvertedIndexEntry : Dictionary<string, FieldMatches>
    {
        public InvertedIndexEntry()
        { }

        public InvertedIndexEntry(int capacity)
            : base(capacity) { }

        public InvertedIndexEntry(IEnumerable<(string term, FieldMatches occurrences)> entries)
            : base(entries.ToDictionary(e => e.term, e => e.occurrences)) { }

        public int Index { get; set; }
    }
}
