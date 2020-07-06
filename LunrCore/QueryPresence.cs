namespace Lunr
{
    /// <summary>
    /// What kind of presence a term must have in matching documents.
    /// </summary>
    public enum QueryPresence
    {
        /// <summary>
        /// Term's presence in a document is optional, this is the default value.
        /// </summary>
        Optional = 0,
        /// <summary>
        /// Term's presence in a document is required, documents that do not contain this term will not be returned.
        /// </summary>
        Required = 1,
        /// <summary>
        /// Term's presence in a document is prohibited, documents that do contain this term will not be returned.
        /// </summary>
        Prohibited = 2
    }
}
