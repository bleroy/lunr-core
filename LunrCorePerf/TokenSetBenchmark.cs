using BenchmarkDotNet.Attributes;
using Lunr;
using System.Linq;

namespace LunrCorePerf
{
    public class TokenSetBenchmark
    {
        private TokenSet _tokenSet = TokenSet.FromArray(new[]
        {
            "january", "february", "march", "april",
            "may", "june", "july", "august",
            "september", "october", "november", "december"
        }.OrderBy(s => s));
        private TokenSet _noWildCard = TokenSet.FromString("september");
        private TokenSet _withWildCard = TokenSet.FromString("*ber");

        private readonly string[] _words = Words.First(1000).OrderBy(w => w).ToArray();

        [Benchmark]
        public void FromArray()
        {
            var _ = TokenSet.FromArray(_words);
        }

        [Benchmark]
        public void FromStringNoWildcard()
        {
            var _ = TokenSet.FromString("javascript");
        }

        [Benchmark]
        public void FromStringWithWildcard()
        {
            var _ = TokenSet.FromString("java*cript");
        }

        [Benchmark]
        public void FromFuzzyString()
        {
            var _ = TokenSet.FromFuzzyString("javascript", 2);
        }

        [Benchmark]
        public void ToArray()
        {
            foreach (string _ in _tokenSet.ToEnumeration()) { }
        }

        [Benchmark]
        public void ToStringToken()
        {
            string _ = _tokenSet.ToString();
        }

        [Benchmark]
        public void IntersectNoWildcard()
        {
            TokenSet _ = _tokenSet.Intersect(_noWildCard);
        }

        [Benchmark]
        public void IntersectWithWildcard()
        {
            TokenSet _ = _tokenSet.Intersect(_withWildCard);
        }
    }
}
