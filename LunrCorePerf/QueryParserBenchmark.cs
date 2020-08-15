using BenchmarkDotNet.Attributes;
using Lunr;

namespace LunrCorePerf
{
    public class QueryParserBenchmark
    {
        private void Parse(string queryString)
        {
            var query = new Query("title", "body");
            var parser = new QueryParser(queryString, query);

            parser.Parse();
        }

        [Benchmark]
        public void ParseSimpleQuery()
        {
            Parse("foo bar");
        }

        [Benchmark]
        public void ParseFieldQuery()
        {
            Parse("title:foo bar");
        }

        [Benchmark]
        public void ParseModifierQuery()
        {
            Parse("foo~2 bar");
        }

        [Benchmark]
        public void ParseComplexQuery()
        {
            Parse("title:foo~2^6 bar");
        }
    }
}
