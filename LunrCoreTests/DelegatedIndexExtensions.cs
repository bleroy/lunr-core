using Lunr;

namespace LunrCoreTests
{
    public static class DelegatedIndexExtensions
    {
        /// <summary> For testing, this wraps a standard in-memory index, so that it can be used as a delegated index. </summary>
        public static DelegatedIndex AsDelegated(this Index index)
        {
            return new DelegatedIndex(key => index.InvertedIndex[key], () => index.FieldVectors.Keys,
                key => index.FieldVectors[key], other => index.TokenSet.Intersect(other), () => index.Fields,
                index.Pipeline);
        }
    }
}