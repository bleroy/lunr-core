using BenchmarkDotNet.Attributes;
using Lunr;

namespace LunrCorePerf
{
    public class TokenizerBenchmark
    {
        private readonly string _lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
            "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
            "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris " +
            "nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in " +
            "reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla " +
            "pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa " +
            "qui officia deserunt mollit anim id est laborum";

        [Benchmark]
        public void TokenizeLipsum()
        {
            foreach (Token _ in new Tokenizer().Tokenize(_lorem)) { }
        }
    }
}
