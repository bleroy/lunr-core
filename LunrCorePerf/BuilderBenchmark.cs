using BenchmarkDotNet.Attributes;
using Lunr;
using System.Threading.Tasks;

namespace LunrCorePerf
{
    public class BuilderBenchmark
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

        [Benchmark]
        public async Task AddDocuments()
            => await Index.Build(config: async builder =>
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
}
