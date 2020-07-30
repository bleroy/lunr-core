using BenchmarkDotNet.Running;

namespace LunrCorePerf
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
