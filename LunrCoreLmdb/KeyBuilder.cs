using System;
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

        #region Field Vectors

        private const string FieldVectorKeyPrefix = "VK:";
        public static byte[] BuildAllFieldVectorKeys() => Encoding.UTF8.GetBytes(FieldVectorKeyPrefix);
        public static ReadOnlySpan<byte> BuildFieldVectorKeyKey(string key) => Encoding.UTF8.GetBytes($"{FieldVectorKeyPrefix}{key}");

        private const string FieldVectorValuePrefix = "V:";
        public static byte[] BuildFieldVectorKey(string key) => Encoding.UTF8.GetBytes($"{FieldVectorValuePrefix}{key}");

        #endregion

        #region Inverted Indices
        
        private const string InvertedIndexEntryPrefix = "E:";

        public static byte[] BuildInvertedIndexEntryKey(string key)
        {
            return Encoding.UTF8.GetBytes($"{InvertedIndexEntryPrefix}{key}");
        }

        #endregion


        
    }
}