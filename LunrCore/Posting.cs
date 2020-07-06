using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Lunr
{
    /// <summary>
    /// Inverted index entry.
    /// field -> document -> metadataKey -> metadata[]
    /// </summary>
    [JsonConverter(typeof(PostingJsonConverter))]
    public class Posting : Dictionary<string, IDictionary<string, IDictionary<string, IList<object>>>>
    {
        public Posting() : base() { }
        public Posting(IEnumerable<(string key, IDictionary<string, IDictionary<string, IList<object>>> val)> entries)
            : base(entries.ToDictionary(e => e.key, e => e.val)) { }

        public int Index { get; set; }
    }
}
