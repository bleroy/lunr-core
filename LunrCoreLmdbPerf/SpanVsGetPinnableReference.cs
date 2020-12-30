using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Lunr;
using LunrCoreLmdb;

namespace LunrCoreLmdbPerf
{
    public class SpanVsGetPinnableReference
    {
        private byte[] _buffer;

        [GlobalSetup]
        public void GlobalSetUp()
        {
            _buffer = VectorFrom(4, 5, 6).Serialize().ToArray();
        }

        [Benchmark]
        public void GetPinnableReference()
        {
            var span = _buffer.AsSpan();

            unsafe
            {
                fixed(byte* buf = &span.GetPinnableReference())
                {
                    var ms = new UnmanagedMemoryStream(buf, _buffer.Length);
                    var br = new BinaryReader(ms);

                    var count = br.ReadInt32();
                    var values = new List<(int, double)>();
                    for (var i = 0; i < count; i++)
                    {
                        var index = br.ReadDouble();
                        var value = br.ReadDouble();
                        values.Add(((int) index, value));
                    }

                    var vector = new Vector(values.ToArray());
                }
            }
        }

        [Benchmark]
        public void Span()
        {
            var span = _buffer.AsSpan();

            var count = BinaryPrimitives.ReadInt32LittleEndian(span);
            span = span[4..];

            var values = new List<(int, double)>();
            for (var i = 0; i < count; i++)
            {
                var index = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(span));
                span = span[8..];

                var value = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(span));
                span = span[8..];

                values.Add(((int) index, value));
            }

            var vector = new Vector(values.ToArray());
        }

        private static Vector VectorFrom(params double[] elements)
            => new Vector(elements.Select((el, i) => (i, el)).ToArray());
    }
}