using System;
using System.Linq;
using System.Threading;
using Lunr;
using LunrCoreLmdb;
using Xunit;
using Index = Lunr.Index;

namespace LunrCoreLmdbTests
{
    public class SerializationTests
    {
        [Fact]
        public void Can_persist_fields()
        {
            const string field = "Field";

            var index = new LmdbIndex(Guid.NewGuid().ToString());

            try
            {
                var addedField = index.AddField(field);
                Assert.True(addedField);

                var fields = index.GetFields();
                Assert.NotNull(fields);
                Assert.Equal(field, fields.Single());

                var removedField = index.RemoveField(field);
                Assert.True(removedField);

                var noFields = index.GetFields();
                Assert.NotNull(fields);
                Assert.Empty(noFields);
            }
            finally
            {
                index.Destroy();
                index.Dispose();
            }
        }

        [Fact]
        public void Can_persist_vectors()
        {
            const string key = "Key";

            var index = new LmdbIndex(Guid.NewGuid().ToString());

            try
            {
                Vector vector = VectorFrom(4, 5, 6);
                Assert.Equal(Math.Sqrt(77), vector.Magnitude);

                var addedVector = index.AddFieldVector(key, vector, CancellationToken.None);
                Assert.True(addedVector);

                var getByKey = index.GetFieldVectorByKey(key);
                Assert.NotNull(getByKey);
                Assert.Equal(Math.Sqrt(77), getByKey?.Magnitude);

                var getKeys = index.GetFieldVectorKeys().ToList();
                Assert.Single(getKeys);
                Assert.Equal(getKeys[0], key);

                var removedVector = index.RemoveFieldVector(key, CancellationToken.None);
                Assert.True(removedVector);

                var noVector = index.GetFieldVectorByKey(key);
                Assert.Null(noVector);

                var noVectorKeys = index.GetFieldVectorKeys(CancellationToken.None);
                Assert.Empty(noVectorKeys);
            }
            finally
            {
                index.Destroy();
                index.Dispose();
            }
        }

        [Fact]
        public void Can_persist_inverted_index_entries()
        {
            var lmdb = new LmdbIndex(Guid.NewGuid().ToString());

            try
            {
                var builder = new Builder();
                builder.AddField("title");
                builder.Add(new Document
                {
                    { "id", "id" },
                    { "title", "test" },
                    { "body", "missing" }
                }).ConfigureAwait(false).GetAwaiter().GetResult();
                Index index = builder.Build();

                var firstKey = index.InvertedIndex.Keys.FirstOrDefault() ?? throw new InvalidOperationException();
                Assert.NotNull(firstKey);

                var added = lmdb.AddInvertedIndexEntry(firstKey, index.InvertedIndex[firstKey], CancellationToken.None);
                Assert.True(added);
                
                var getInvertedIndexEntry = lmdb.GetInvertedIndexEntryByKey(firstKey);
                Assert.NotNull(getInvertedIndexEntry);

                var tokenSet = lmdb.IntersectTokenSets(index.TokenSet);
                Assert.Single(tokenSet.Edges);
            }
            finally
            {
                lmdb.Destroy();
                lmdb.Dispose();
            }
        }

        [Fact]
        public void Can_round_trip_vectors()
        {
            Vector original = VectorFrom(4, 5, 6);
            Assert.Equal(Math.Sqrt(77), original.Magnitude);
            
            var buffer = original.Serialize();
            var deserialized = buffer.DeserializeFieldVector();

            Assert.NotSame(original, deserialized);
            Assert.Equal(Math.Sqrt(77), deserialized.Magnitude);
        }

        [Fact]
        public void Can_round_trip_inverted_indexes()
        {
            var builder = new Builder();
            builder.AddField("title");
            builder.Add(new Document
            {
                { "id", "id" },
                { "title", "test" },
                { "body", "missing" }
            }).ConfigureAwait(false).GetAwaiter().GetResult();
            Index index = builder.Build();

            var original = index.InvertedIndex;
            var buffer = original.Serialize();
            var deserialized = buffer.DeserializeInvertedIndex();

            AssertInvertedIndex(original, deserialized);
        }

        [Fact]
        public void Can_round_trip_inverted_index_entries()
        {
            var builder = new Builder();
            builder.AddField("title");
            builder.Add(new Document
            {
                { "id", "id" },
                { "title", "test" },
                { "body", "missing" }
            }).ConfigureAwait(false).GetAwaiter().GetResult();
            Index index = builder.Build();

            foreach (var (_, original) in index.InvertedIndex)
            {
                var buffer = original.Serialize();
                var deserialized = buffer.DeserializeInvertedIndexEntry();

                AssertInvertedIndexEntry(original, deserialized);
            }
        }

        [Fact]
        public void Can_round_trip_token_set()
        {
            var builder = new Builder();
            builder.AddField("title");
            builder.Add(new Document
            {
                { "id", "id" },
                { "title", "test" },
                { "body", "missing" }
            }).ConfigureAwait(false).GetAwaiter().GetResult();
            Index index = builder.Build();

            var original = index.TokenSet;
            var deserialized = original.Serialize().DeserializeTokenSet();

            Assert.NotSame(original, deserialized);
            Assert.Equal(original.ToEnumeration(), deserialized.ToEnumeration());
        }

        private static void AssertInvertedIndex(InvertedIndex left, InvertedIndex right)
        {
            Assert.Equal(left.Count, right.Count);

            var all = left.Zip(right, (entriesLeft, entriesRight) =>
            {
                Assert.Equal(entriesLeft.Key, entriesRight.Key);
                AssertInvertedIndexEntry(entriesLeft.Value, entriesRight.Value);
                return true;
            }).ToList();

            Assert.All(all, Assert.True);
        }

        private static void AssertInvertedIndexEntry(InvertedIndexEntry left, InvertedIndexEntry right)
        {
            Assert.Equal(left.Index, right.Index);
            Assert.Equal(left.Count, right.Count);

            var allFieldMatches = left.Zip(right, (fieldMatchesLeft, fieldMatchesRight) =>
            {
                Assert.Equal(fieldMatchesLeft.Key, fieldMatchesRight.Key);
                Assert.Equal(fieldMatchesLeft.Value.Count, fieldMatchesRight.Value.Count);

                var allFieldMatchMetadata = fieldMatchesLeft.Value.Zip(fieldMatchesRight.Value, (fieldMatchMetadataLeft, fieldMatchMetadataRight) =>
                {
                    Assert.Equal(fieldMatchMetadataLeft.Key, fieldMatchMetadataRight.Key);
                    return true;
                }).ToList();

                Assert.All(allFieldMatchMetadata, Assert.True);
                return true;
            }).ToList();

            Assert.All(allFieldMatches, Assert.True);
        }

        private static Vector VectorFrom(params double[] elements)
            => new Vector(elements.Select((el, i) => (i, el)).ToArray());
    }
}