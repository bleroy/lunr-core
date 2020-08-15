using BenchmarkDotNet.Attributes;
using Lunr;
using System.Linq;

namespace LunrCorePerf
{
    public class StemmerBenchmark
    {
        private readonly StemmerBase _stemmer = new EnglishStemmer();
        private readonly string[] _words = Words.First(1000).OrderBy(w => w).ToArray();

        [Benchmark]
        public void StemEnglishWords()
        {
            foreach(string word in _words)
            {
                _stemmer.Stem(word);
            }
        }
    }
}
