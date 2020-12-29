using Lunr;

namespace LunrCoreLmdb
{
    public static class Lmdb
    {
        public static DelegatedIndex Open(string path, Pipeline pipeline)
        {
            var index = new LmdbIndex(path);

            return new DelegatedIndex(index, pipeline);
        }
    }
}