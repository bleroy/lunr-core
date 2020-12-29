using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LunrCoreLmdb;

namespace LunrCoreLmdbPerf
{
    public class SearchBenchmarkWrappedIndex : SearchBenchmarkBase
    {
        [GlobalSetup]
        public async Task Setup()
        {
            Index = (await PlainIndex()).AsDelegated();
        }
    }
}