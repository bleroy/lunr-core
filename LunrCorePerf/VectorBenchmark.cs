using BenchmarkDotNet.Attributes;
using Lunr;
using System;

namespace LunrCorePerf
{
    public class VectorBenchmark
    {
        private readonly Vector _v1 = new Vector();
        private readonly Vector _v2 = new Vector();
        private readonly Random _rnd = new Random();

        [GlobalSetup]
        public void Setup()
        {
            for (int i = 0; i < 1000; i++)
            {
                int index = _rnd.Next(i, i + 100);
                double val = _rnd.NextDouble() * 100;
                _v1.Insert(index, val);
            }

            for (int i = 0; i < 1000; i++)
            {
                int index = _rnd.Next(i, i + 100);
                double val = _rnd.NextDouble() * 100;
                _v2.Insert(index, val);
            }
        }

        [Benchmark]
        public void Magnitude()
        {
            double _ = _v1.Magnitude;
        }

        [Benchmark]
        public void Dot()
        {
            double _ = _v1.Dot(_v2);
        }

        [Benchmark]
        public void Similarity()
        {
            double _ = _v1.Similarity(_v2);
        }
    }
}
