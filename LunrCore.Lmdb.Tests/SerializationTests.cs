using System;
using System.Linq;
using Lunr;
using Xunit;
using Index = Lunr.Index;

namespace LunrCore.Lmdb.Tests
{
    public class SerializationTests
    {
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

                AssertInvertedIndexValue(original, deserialized);
            }
        }

        private static void AssertInvertedIndex(InvertedIndex left, InvertedIndex right)
        {
            Assert.Equal(left.Count, right.Count);

            var all = left.Zip(right, (entriesLeft, entriesRight) =>
            {
                Assert.Equal(entriesLeft.Key, entriesRight.Key);
                AssertInvertedIndexValue(entriesLeft.Value, entriesRight.Value);
                return true;
            }).ToList();

            Assert.All(all, Assert.True);
        }


        private static void AssertInvertedIndexValue(InvertedIndexEntry left, InvertedIndexEntry right)
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