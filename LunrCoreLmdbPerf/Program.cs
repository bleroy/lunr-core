using BenchmarkDotNet.Running;

namespace LunrCoreLmdbPerf
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
