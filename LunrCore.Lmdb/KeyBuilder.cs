using System.Text;
using Lunr;

namespace LunrCore.Lmdb
{
    internal static class KeyBuilder
    {
        #region Fields

        private const string FieldPrefix = "F:";

        public static byte[] BuildAllFieldsKey() => Encoding.UTF8.GetBytes(FieldPrefix);

        public static byte[] BuildFieldKey(string field)
        {
            return Encoding.UTF8.GetBytes($"{FieldPrefix}{field}");
        }

        #endregion

        #region Vectors

        private const string VectorPrefix = "V:";

        public static byte[] BuildVectorKey(string key)
        {
            return Encoding.UTF8.GetBytes($"{VectorPrefix}{key}");
        }

        #endregion

        #region Inverted Indices

        private const string InvertedIndexPrefix = "I:";

        public static byte[] BuildInvertedIndexKey(string key)
        {
            return Encoding.UTF8.GetBytes($"{InvertedIndexPrefix}{key}");
        }

        #endregion
    }
}