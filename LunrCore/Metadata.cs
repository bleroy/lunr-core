using System.Collections.Generic;

namespace Lunr
{
    public class Metadata : Dictionary<string, IList<object>>
    {
        public Metadata() : base() { }
        public Metadata(int capacity) : base(capacity) { }
    }
}
