using System;
using System.Linq;
using System.Threading.Tasks;
using Lunr;
using LunrCoreLmdb;
using Xunit;
using Index = Lunr.Index;

namespace LunrCoreLmdbTests
{
    public class SearchTests
    {
        [Fact]
        public async void Can_populate_from_existing_index() => await WithDelegatedIndex(Assert.NotNull);

        [Fact]
        public async Task SearchWithBuildTimeFieldBoostsNoQueryBoost()
        {
            await WithDelegatedIndex(async idx =>
            {
                Result result = (await idx.Search("professor").ToList()).First();
                // b ranks highest
                Assert.Equal("b", result.DocumentReference);
            });
        }

        private async Task WithDelegatedIndex(Action<DelegatedIndex> action)
        {
            var plain = await GetPlainIndex();

            var path = $"{Guid.NewGuid()}";
            var lmdb = new LmdbIndex(path);

            try
            {
                foreach (var field in plain.Fields)
                    lmdb.AddField(field);

                foreach (var (k, v) in plain.FieldVectors)
                    lmdb.AddFieldVector(k, v);

                foreach (var (k, v) in plain.InvertedIndex)
                    lmdb.AddInvertedIndexEntry(k, v);

                var index = Lmdb.Open(path, plain.Pipeline);

                action?.Invoke(index);
            }
            finally
            {
                lmdb.Dispose();
            }
        }

        private readonly Document[] _documents = {
            new Document
            {
                { "id", "a" },
                { "title", "Mr. Green kills Colonel Mustard" },
                { "body", "Mr. Green killed Colonel Mustard in the study with the candlestick. Mr. Green is not a very nice fellow." },
                { "wordCount", 19 }
            },
            new Document
            {
                { "id", "b" },
                { "title", "Plumb waters plant" },
                { "body", "Professor Plumb has a green plant in his study" },
                { "wordCount", 9 }
            },
            new Document
            {
                { "id", "c" },
                { "title", "Scarlett helps Professor" },
                { "body", "Miss Scarlett watered Professor Plumbs green plant while he was away from his office last week." },
                { "wordCount", 16 }
            }
        };

        private async Task<Index> GetPlainIndex()
        {
            return await Index.Build(async builder =>
            {
                builder.ReferenceField = "id";

                builder
                    .AddField("title")
                    .AddField("body", 10);

                foreach (Document doc in _documents)
                {
                    await builder.Add(doc);
                }
            });
        }
    }
}