using System.Collections.Generic;
using Lunr;

namespace LunrCoreLmdb
{
    public static class Delegates
    {
        public delegate InvertedIndexEntry? GetInvertedIndexEntryByKey(string key);

        public delegate IEnumerable<string> GetFieldVectorKeys();

        public delegate Vector? GetFieldVectorByKey(string key);

        public delegate TokenSet IntersectTokenSets(TokenSet other);

        public delegate IEnumerable<string> GetFields();
    }
}
