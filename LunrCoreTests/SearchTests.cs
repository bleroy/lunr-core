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

        [Fact]
        public async Task MultipleTermsOneTermsMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("week foo").ToList();

            Assert.Single(results);
            Assert.Equal("c", results[0].DocumentReference);
            Assert.Equal(new[] { "week" }, results[0].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task MultipleTermsDuplicateQueryTerms()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("fellow candlestick foo bar green plant fellow").ToList();

            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task MultipleTermsDocumentsWithAllTermsScoreHigher()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("candlestick green").ToList();

            Assert.Equal(3, results.Count);
            Assert.Equal(new[] { "a", "b", "c" }, results.Select(result => result.DocumentReference));
            Assert.Equal(new[] { "candlestick", "green" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal("green", results[1].MatchData.Metadata.Keys.Single());
            Assert.Equal("green", results[2].MatchData.Metadata.Keys.Single());
        }

        [Fact]
        public async Task MultipleTermsNoTermsMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("foo bar").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task CorpusTermsAreStemmed()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("water").ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(new[] { "b", "c" }.ToHashSet(), results.Select(result => result.DocumentReference).ToHashSet());
        }

        [Fact]
        public async Task FieldScopedTerm()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("title:plant").ToList();

            Assert.Single(results);
            Assert.Equal("b", results[0].DocumentReference);
            Assert.Equal("plant", results[0].MatchData.Metadata.Keys.Single());
            Assert.Equal("title", results[0].MatchData.Metadata["plant"].Keys.Single());
        }

        [Fact]
        public async Task FieldScopedTermNoMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("title:candlestick").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task TrailingWildcardNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("fo*").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task TrailingWildcardOneMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("candle*").ToList();

            Assert.Single(results);
            Assert.Equal("a", results[0].DocumentReference);
            Assert.Equal("candlestick", results[0].MatchData.Metadata.Keys.Single());
        }

        [Fact]
        public async Task TrailingWildcardMultipleMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("pl*").ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(new[] { "b", "c" }, results.Select(result => result.DocumentReference));
            Assert.Equal(new[] { "plumb", "plant" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal(new[] { "plumb", "plant" }, results[1].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task LeadingWildcardNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("*oo").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task LeadingWildcardMultipleMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("*ant").ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(new[] { "b", "c" }, results.Select(result => result.DocumentReference));
            Assert.Equal(new[] { "plant" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal(new[] { "plant" }, results[1].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task ContainedWildcardNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("f*o").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task ContainedWildcardMultipleMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("pl*nt").ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(new[] { "b", "c" }, results.Select(result => result.DocumentReference));
            Assert.Equal(new[] { "plant" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal(new[] { "plant" }, results[1].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task EditDistanceNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("foo~1").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task EditDistanceMultipleMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("plont~1").ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(new[] { "b", "c" }, results.Select(result => result.DocumentReference));
            Assert.Equal(new[] { "plant" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal(new[] { "plant" }, results[1].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task SearchByUnknownField()
        {
            Index idx = await GetPlainIndex();

            await Assert.ThrowsAsync<QueryParserException>(async () =>
            {
                await idx.Search("unknown-field:plant").ToList();
            });
        }

        [Fact]
        public async Task SearchByFieldNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("title:candlestick").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task SearchByFieldOneMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("title:plant").ToList();

            Assert.Single(results);
            Assert.Equal("b", results[0].DocumentReference);
            Assert.Equal("plant", results[0].MatchData.Metadata.Keys.Single());
        }

        [Fact]
        public async Task BoostNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("foo^10").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task BoostMultipleMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("scarlett candlestick^5").ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(new[] { "a", "c" }, results.Select(result => result.DocumentReference));
            Assert.Equal(new[] { "candlestick" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal(new[] { "scarlett" }, results[1].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task TypeAheadStyleSearchNoResults()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Query(q => q
                .AddTerm("xyz", boost: 100, usePipeline: true)
                .AddTerm("xyz", boost: 1, usePipeline: false, wildcard: QueryWildcard.Trailing)
                .AddTerm("xyz", boost: 1, editDistance: 1)
            ).ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task TypeAheadStyleSearchMultipleResults()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Query(q => q
                .AddTerm("pl", boost: 100, usePipeline: true)
                .AddTerm("pl", boost: 1, usePipeline: false, wildcard: QueryWildcard.Trailing)
                .AddTerm("pl", boost: 1, editDistance: 1)
            ).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(new[] { "b", "c" }, results.Select(result => result.DocumentReference));
            Assert.Equal(new[] { "plumb", "plant" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal(new[] { "plumb", "plant" }, results[1].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task ProhibitedTermNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("-green").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task ProhibitedTermMultipleMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("-candlestick green").ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(new[] { "b", "c" }, results.Select(result => result.DocumentReference));
            Assert.Equal(new[] { "green" }, results[0].MatchData.Metadata.Keys);
            Assert.Equal(new[] { "green" }, results[1].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task NegatedTermNoMatches()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("-qwertyuiop").ToList();

            Assert.Equal(3, results.Count);
            Assert.True(results.All(result => result.Score == 0));
        }

        [Fact]
        public async Task NegatedTermSomeMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("-plant").ToList();

            Assert.Single(results);
            Assert.Equal(0, results[0].Score);
            Assert.Equal("a", results[0].DocumentReference);
        }

        [Fact]
        public async Task FieldMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("-title:plant plumb").ToList();

            Assert.Single(results);
            Assert.Equal("c", results[0].DocumentReference);
            Assert.Equal("plumb", results[0].MatchData.Metadata.Keys.Single());
        }

        [Fact]
        public async Task RequiredTermMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("+candlestick green").ToList();

            Assert.Single(results);
            Assert.Equal("a", results[0].DocumentReference);
            Assert.Equal(new[] { "candlestick", "green" }, results[0].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task RequiredTermsNoMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("+mustard +plant").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task NoMatchingTerms()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("+qwertyuiop green").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task RequiredFieldMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("+title:plant green").ToList();

            Assert.Single(results);
            Assert.Equal("b", results[0].DocumentReference);
            Assert.Equal(new[] { "plant", "green" }, results[0].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task RequiredFieldAndTermMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("+title:plant +green").ToList();

            Assert.Single(results);
            Assert.Equal("b", results[0].DocumentReference);
            Assert.Equal(new[] { "plant", "green" }, results[0].MatchData.Metadata.Keys);
        }

        [Fact]
        public async Task TwoRequiredFieldsMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("+title:plant +body:study").ToList();

            Assert.Single(results);
            Assert.Equal("b", results[0].DocumentReference);
            Assert.Equal(new[] { "studi", "plant" }.ToHashSet(), results[0].MatchData.Metadata.Keys.ToHashSet());
        }

        [Fact]
        public async Task TwoRequiredFieldsOnlyOneMatch()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("+title:plant +body:qwertyuiop").ToList();

            Assert.Empty(results);
        }

        [Fact]
        public async Task AllTogetherNow()
        {
            Index idx = await GetPlainIndex();

            IList<Result> results = await idx.Search("+plant green -office").ToList();

            Assert.Single(results);
            Assert.Equal("b", results[0].DocumentReference);
            Assert.Equal(new[] { "plant", "green" }, results[0].MatchData.Metadata.Keys);
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
