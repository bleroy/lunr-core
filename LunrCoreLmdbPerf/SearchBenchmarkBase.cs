using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Lunr;
using LunrCoreLmdb;

namespace LunrCoreLmdbPerf
{
    public abstract class SearchBenchmarkBase
    {
        protected DelegatedIndex Index;

        protected readonly Document[] Documents = {
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

        [Benchmark]
        public async Task SearchSingleTerm()
        {
            await foreach (Result _ in Index.Search("green")) { }
        }

        [Benchmark]
        public async Task SearchMultipleTerms()
        {
            await foreach (Result _ in Index.Search("green plant")) { }
        }

        [Benchmark]
        public async Task SearchTrailingWildcard()
        {
            await foreach (Result _ in Index.Search("pl*")) { }
        }

        [Benchmark]
        public async Task SearchLeadingWildcard()
        {
            await foreach (Result _ in Index.Search("*ant")) { }
        }

        [Benchmark]
        public async Task SearchContainedWildcard()
        {
            await foreach (Result _ in Index.Search("p*t")) { }
        }

        [Benchmark]
        public async Task SearchWithField()
        {
            await foreach (Result _ in Index.Search("title:plant")) { }
        }

        [Benchmark]
        public async Task SearchWithEditDistance()
        {
            await foreach (Result _ in Index.Search("plint~2")) { }
        }

        [Benchmark]
        public async Task SearchTypeAhead()
        {
            await foreach (Result _ in Index.Query(q =>
            {
                q.AddTerm("pl", boost: 100, usePipeline: true);
                q.AddTerm("pl", boost: 10, usePipeline: false, wildcard: QueryWildcard.Trailing);
                q.AddTerm("pl", boost: 1, editDistance: 1);
            })) { }
        }

        [Benchmark]
        public async Task SearchNegatedQuery()
        {
            await foreach (Result _ in Index.Search("-plant")) { }
        }

        [Benchmark]
        public async Task SearchRequiredTerm()
        {
            await foreach (Result _ in Index.Search("green +plant")) { }
        }

        public async Task<Index> PlainIndex()
        {
            var index = await Lunr.Index.Build(config: async builder =>
            {
                builder.ReferenceField = "id";

                builder
                    .AddField("title")
                    .AddField("body", boost: 10);

                foreach (Document doc in Documents)
                {
                    await builder.Add(doc);
                }
            });

            return index;
        }
    }
}