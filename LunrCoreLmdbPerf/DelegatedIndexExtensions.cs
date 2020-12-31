using System.Collections.Generic;
using Lunr;
using LunrCoreLmdb;

namespace LunrCoreLmdbPerf
{
    internal static class DelegatedIndexExtensions
    {
        /// <summary> This wraps a standard in-memory index, so that it can be used as a delegated index. </summary>
        public static DelegatedIndex AsDelegated(this Index index) => new DelegatedIndex(new ReadOnlyIndex(index), index.Pipeline);

        internal class ReadOnlyIndex : IReadOnlyIndex
        {
            private readonly Index _index;

            public ReadOnlyIndex(Index index) => _index = index;
            public InvertedIndexEntry? GetInvertedIndexEntryByKey(string key) => _index.InvertedIndex[key];
            public IEnumerable<string> GetFieldVectorKeys() => _index.FieldVectors.Keys;
            public Vector? GetFieldVectorByKey(string key) => _index.FieldVectors[key];
            public TokenSet IntersectTokenSets(TokenSet other) => _index.TokenSet.Intersect(other);
            public IEnumerable<string> GetFields() => _index.Fields;
        }
    }
}