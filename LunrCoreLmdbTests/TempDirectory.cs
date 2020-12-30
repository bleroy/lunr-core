using System;
using System.Diagnostics;
using System.IO;

namespace LunrCoreLmdbTests
{
    public class TempDirectory : IDisposable
    {
        private readonly string _directory;

        public TempDirectory()
        {
            _directory = Path.Combine(Directory.GetCurrentDirectory(), "lmdb");
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_directory))
                {
                    Directory.Delete(_directory, true);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        public string NewDirectory()
        {
            var path = Path.Combine(_directory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);
            return path;
        }
    }
}