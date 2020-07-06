using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Lunr
{
    /// <summary>
    /// Inverted index.
    /// term -> field -> document -> metadataKey -> metadata[]
    /// </summary>
    [JsonConverter(typeof(InvertedIndexJsonConverter))]
    public class InvertedIndex : Dictionary<string, Posting>
    {
        public InvertedIndex() : base() { }

        public InvertedIndex(IEnumerable<(string term, Posting posting)> entries)
            : base(entries.ToDictionary(e => e.term, e => e.posting)) { }
    }
}
