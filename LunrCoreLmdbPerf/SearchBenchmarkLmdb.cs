using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LunrCoreLmdb;

namespace LunrCoreLmdbPerf
{
    public class SearchBenchmarkLmdb : SearchBenchmarkBase
    {
        private string _path;
        private LmdbIndex _lmdb;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _path = Guid.NewGuid().ToString();

            var plain = await PlainIndex();

            _lmdb = new LmdbIndex(_path);

            foreach (var field in plain.Fields)
                _lmdb.AddField(field);

            foreach (var (k, v) in plain.FieldVectors)
                _lmdb.AddFieldVector(k, v);

            foreach (var (k, v) in plain.InvertedIndex)
                _lmdb.AddInvertedIndexEntry(k, v);

            Index = Lmdb.Open(_path, plain.Pipeline);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            Index.Dispose();

            try
            {
                Directory.Delete(_path, recursive: true);
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.ToString());
            }
        }
    }
}