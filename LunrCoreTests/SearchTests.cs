using Lunr;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LunrCoreTests
{
    public class SearchTests
    {
        private readonly Document[] _documents = new[]
        {
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

        [Fact]
        public async Task SearchWithBuildTimeFieldBoostsNoQueryBoost()
        {
            Index idx = await GetPlainIndex();

            Result result = (await idx.Search("professor").ToList()).First();
            // b ranks highest
            Assert.Equal("b", result.DocumentReference);
        }

        [Fact]
        public async Task SearchWithBuildTimeDocumentBoostsNoQueryBoost()
        {
            Index idx = await GetIndexWithDocumentBoost();

            Result result = (await idx.Search("plumb").ToList()).First();
            // c ranks highest
            Assert.Equal("c", result.DocumentReference);
        }

        [Fact]
        public async Task SearchWithWithBuildTimeDocumentBoostsAndQueryBoost()
        {
            Index idx = await GetIndexWithDocumentBoost();

            Result result = (await idx.Search("green study^10").ToList()).First();
            // b ranks highest
            Assert.Equal("b", result.DocumentReference);
        }

        [Fact]
        public async Task SingleTermSearchWithoutBuildTimeBoost()
        {
            Index idx = await GetPlainIndex();

            Result result = (await idx.Search("scarlett").ToList()).Single();
            Assert.Equal("c", result.DocumentReference);
        }

        [Fact]
        public async Task SearchNoMatch()
        {
            Index idx = await GetPlainIndex();

            Assert.False(await idx.Search("foo").Any());
        }

        [Fact]
        public async Task SearchMultipleMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("plant").ToList();
            
            Assert.Equal(2, results.Count);
            Assert.Equal("b", results[0].DocumentReference);
            Assert.Equal("c", results[1].DocumentReference);
        }

        // study would be stemmed to studi, tokens
        // are stemmed by default on index and must
        // also be stemmed on search to match
        [Fact]
        public async Task PipelineProcessingTwoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Query(q =>
            {
                q.AddTerm(term: "study", usePipeline: true);
            }).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal("b", results[0].DocumentReference);
            Assert.Equal("a", results[1].DocumentReference);
        }

        [Fact]
        public async Task NoPipelineProcessingNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Query(q =>
            {
                q.AddTerm(term: "study", usePipeline: false);
            }).ToList();

            Assert.False(results.Any());
        }

        [Fact]
        public async Task MultipleTermsAllTermsMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("fellow candlestick").ToList();

            Assert.Single(results);
            Assert.Equal("a", results[0].DocumentReference);
            Assert.Equal(new[] { "fellow", "candlestick" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal("body", results[0].MatchData.Metadata["fellow"].Keys.Single());
            Assert.Equal("body", results[0].MatchData.Metadata["candlestick"].Keys.Single());
        }

        private async Task<Index> GetPlainIndex()
        {
            return await Index.Build(config: async builder =>
            {
                builder.ReferenceField = "id";

                builder
                    .AddField("title")
                    .AddField("body", boost: 10);

                foreach (Document doc in _documents)
                {
                    await builder.Add(doc);
                }
            });
        }

        private async Task<Index> GetIndexWithDocumentBoost()
        {
            return await Index.Build(config: async builder =>
            {
                builder.ReferenceField = "id";

                builder
                    .AddField("title")
                    .AddField("body");

                foreach (Document doc in _documents)
                {
                    if (doc["id"] as string == "c")
                    {
                        doc.Boost = 10;
                    }
                    await builder.Add(doc);
                }
            });
        }
    }
}
