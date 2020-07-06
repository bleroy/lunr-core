using System;

namespace Lunr
{
    /// <summary>
    /// What kind of automatic wildcard insertion will be used when constructing a query clause.
    /// 
    /// This allows wildcards to be added to the beginning and end of a term without having to manually do any string
    /// concatenation.
    ///
    /// The wildcards can be combined to select both leading and trailing wildcards.
    /// </summary>
    [Flags]
    public enum QueryWildcard
    {
        None = 0,
        Leading = 1,
        Trailing = 2,
        Both = 3
    }
}
