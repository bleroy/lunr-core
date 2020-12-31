using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace LunrCoreLmdbPerf
{
    [SimpleJob(RunStrategy.Throughput)]
    public class InterpolateVsAdd
    {
        [Benchmark]
        public void Interpolate()
        {
            const int a = 1;
            const int b = 2;
            Encoding.UTF8.GetBytes($"{a}/{b}");
        }

        [Benchmark]
        public void Add()
        {
            const int a = 1;
            const int b = 2;
            Encoding.UTF8.GetBytes(a + "/" + b);
        }
    }
}