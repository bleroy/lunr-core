using BenchmarkDotNet.Attributes;
using Lunr;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LunrCorePerf
{
    public class PipelineBenchmark
    {
        private static readonly string[] _first1000Words = Words.First(1000);
        private Pipeline _tokenToTokenPipeline;
        private Pipeline _tokenToTokenArrayPipeline;
        private IEnumerable<Token> _fewTokens;
        private IEnumerable<Token> _manyTokens;

        [GlobalSetup]
        public void Setup()
        {
            _fewTokens = BuildTokens(50);
            _manyTokens = BuildTokens(1000);
            _tokenToTokenPipeline = new Pipeline(Pipeline.BuildFunction(TokenToToken));
            _tokenToTokenArrayPipeline = new Pipeline(Pipeline.BuildFunction(TokenToTokenArray));
        }

        private static IEnumerable<Token> BuildTokens(int count)
            => _first1000Words.Take(count).Select(word => new Token(word));

        private static Token[] TokenToTokenArray(Token token) => new[] { token, token };

        private static Token TokenToToken(Token token) => token;

        [Benchmark]
        public async Task FewTokensTokenToTokenPipeline()
        {
            var cToken = new CancellationToken();
            await foreach(Token _ in _tokenToTokenPipeline.Run(_fewTokens.ToAsyncEnumerable(cToken), cToken)) { }
        }

        [Benchmark]
        public async Task ManyTokensTokenToTokenPipeline()
        {
            var cToken = new CancellationToken();
            await foreach (Token _ in _tokenToTokenPipeline.Run(_manyTokens.ToAsyncEnumerable(cToken), cToken)) { }
        }

        [Benchmark]
        public async Task FewTokensTokenToTokenArrayPipeline()
        {
            var cToken = new CancellationToken();
            await foreach (Token _ in _tokenToTokenArrayPipeline.Run(_fewTokens.ToAsyncEnumerable(cToken), cToken)) { }
        }

        [Benchmark]
        public async Task ManyTokensTokenToTokenArrayPipeline()
        {
            var cToken = new CancellationToken();
            await foreach (Token _ in _tokenToTokenArrayPipeline.Run(_manyTokens.ToAsyncEnumerable(cToken), cToken)) { }
        }
    }
}
