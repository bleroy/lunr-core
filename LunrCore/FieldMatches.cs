using System.Collections.Generic;

namespace Lunr
{
    /// <summary>
    /// Represents a set of matches for a field.
    /// The key is a token, and the value is the metadata associated with this token match for this field.
    /// </summary>
    public class FieldMatches : Dictionary<string, FieldMatchMetadata>
    {
    }
}
