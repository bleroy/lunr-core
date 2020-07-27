using System.Collections.Generic;
using System.Linq;

namespace Lunr
{
    /// <summary>
    /// Represents the metadata associated with a token.
    /// </summary>
    public class TokenMetadata : Dictionary<string, object?>
    {
        public TokenMetadata() : base() { }
        public TokenMetadata(int capacity) : base(capacity) { }
        public TokenMetadata(IEnumerable<(string key, object? value)> data) : base(data.ToDictionary(d => d.key, d => d.value)) { }
        public TokenMetadata(IDictionary<string, object?> data) : base(data) { }
    }
}
