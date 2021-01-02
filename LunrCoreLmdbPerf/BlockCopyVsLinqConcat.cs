using System;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace LunrCoreLmdbPerf
{
    [SimpleJob(RunStrategy.Throughput)]
    public class BlockCopyVsLinqConcat
    {
        private byte[] _left = null!;
        private byte[] _right = null!;

        [GlobalSetup]
        public void GlobalSetUp()
        {
            _left = Encoding.UTF8.GetBytes("T:");
            _right = Encoding.UTF8.GetBytes("word");
        }

        [Benchmark]
        public void BlockCopy()
        {
            var buffer = new byte[_left.Length + _right.Length];
            Buffer.BlockCopy(_left, 0, buffer, 0, _left.Length);
            Buffer.BlockCopy(_right, 0, buffer, _left.Length, _right.Length);
        }

        [Benchmark]
        public void LinqConcat()
        {
            var buffer = _left.Concat(_right).ToArray();
        }
    }
}