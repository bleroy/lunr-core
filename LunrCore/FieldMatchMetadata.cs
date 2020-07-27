using System.Collections.Generic;

namespace Lunr
{
    /// <summary>
    /// The metadata associated with a token match on a field.
    /// The keys are the metadata entry names, the values are lists of values.
    /// </summary>
    public class FieldMatchMetadata : Dictionary<string, IList<object?>>
    {
        public FieldMatchMetadata() : base() { }
        public FieldMatchMetadata(int capacity) : base(capacity) { }
    }
}
