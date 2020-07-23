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
    public class InvertedIndexEntry : Dictionary<string, FieldOccurrences>
    {
        public InvertedIndexEntry() : base() { }
        public InvertedIndexEntry(IEnumerable<(string term, FieldOccurrences occurrences)> entries)
            : base(entries.ToDictionary(e => e.term, e => e.occurrences)) { }

        public int Index { get; set; }
    }
}
