using System.Collections.Generic;

namespace Lunr
{
    public sealed class Document : Dictionary<string, object>
    {
        public Document() : base() { }

        public Document(IDictionary<string, object> dict) : base(dict) { }

        public double Boost { get; set; } = 1;
    }
}
