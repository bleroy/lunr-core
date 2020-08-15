using BenchmarkDotNet.Attributes;
using Lunr;
using System.Threading.Tasks;

namespace LunrCorePerf
{
    public class SearchBenchmark
    {
        private Index _index;

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

        [GlobalSetup]
        public async Task Setup()
        {
            _index = await Index.Build(config: async builder =>
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

        [Benchmark]
        public async Task SearchSingleTerm()
        {
            await foreach (Result _ in _index.Search("green")) { }
        }

        [Benchmark]
        public async Task SearchMultipleTerms()
        {
            await foreach (Result _ in _index.Search("green plant")) { }
        }

        [Benchmark]
        public async Task SearchTrailingWildcard()
        {
            await foreach (Result _ in _index.Search("pl*")) { }
        }

        [Benchmark]
        public async Task SearchLeadingWildcard()
        {
            await foreach (Result _ in _index.Search("*ant")) { }
        }

        [Benchmark]
        public async Task SearchContainedWildcard()
        {
            await foreach (Result _ in _index.Search("p*t")) { }
        }

        [Benchmark]
        public async Task SearchWithField()
        {
            await foreach (Result _ in _index.Search("title:plant")) { }
        }

        [Benchmark]
        public async Task SearchWithEditDistance()
        {
            await foreach (Result _ in _index.Search("plint~2")) { }
        }

        [Benchmark]
        public async Task SearchTypeAhead()
        {
            await foreach (Result _ in _index.Query(q =>
            {
                q.AddTerm("pl", boost: 100, usePipeline: true);
                q.AddTerm("pl", boost: 10, usePipeline: false, wildcard: QueryWildcard.Trailing);
                q.AddTerm("pl", boost: 1, editDistance: 1);
            })) { }
        }

        [Benchmark]
        public async Task SearchNegatedQuery()
        {
            await foreach (Result _ in _index.Search("-plant")) { }
        }

        [Benchmark]
        public async Task SearchRequiredTerm()
        {
            await foreach (Result _ in _index.Search("green +plant")) { }
        }
    }
}
