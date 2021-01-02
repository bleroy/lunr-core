using Lunr;
using LunrCoreLmdb;
using Xunit;

namespace LunrCoreLmdbTests
{
    public static class IndexExtensions
    {
        public static DelegatedIndex CopyToLmdb(this Index index, string path)
        {
            var lmdb = new LmdbIndex(path);
            
            foreach (var field in index.Fields)
                Assert.True(lmdb.AddField(field));

            foreach (var (k, v) in index.FieldVectors)
                Assert.True(lmdb.AddFieldVector(k, v));

            foreach (var (k, v) in index.InvertedIndex)
                Assert.True(lmdb.AddInvertedIndexEntry(k, v));

            var idx = new DelegatedIndex(lmdb, index.Pipeline);

            return idx;
        }
    }
}