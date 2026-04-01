using BenchmarkDotNet.Attributes;
using Lunr;

namespace LunrCorePerf
{
    [MemoryDiagnoser]
    public class TrimmerBenchmark
    {
        private static readonly string[] TestInputs = new[]
        {
            "hello",
            "hello.",
            "it's",
            "james'",
            "stop!'",
            "first,'",
            "[tag]'",
            "__Proto__'"
        };

        private Trimmer _trimmer = null!;

        [GlobalSetup]
        public void Setup()
        {
            _trimmer = new Trimmer();
        }

        [Benchmark]
        public void TrimAll()
        {
            foreach (string input in TestInputs)
            {
                _trimmer.Trim(input);
            }
        }
    }
}