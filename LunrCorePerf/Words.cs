using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunrCorePerf
{
    public static class Words
    {
        public static string[] First(int n)
            => Lines(n).ToArray();

        private static IEnumerable<string> Lines(int limit = 0)
        {
            int count = 0;
            using FileStream file = File.OpenRead(Path.Combine("fixtures", "words.txt"));
            using var reader = new StreamReader(file, true);
            while (!reader.EndOfStream && count++ < limit)
            {
                yield return reader.ReadLine();
            }
        }
    }
}
