using System;
using System.Text;

namespace LunrCoreLmdb
{
    internal static class KeyBuilder
    {
        #region Fields

        private static readonly byte[] FieldPrefix = Encoding.UTF8.GetBytes("F:");
        public static byte[] GetAllFieldsKey() => FieldPrefix;
        public static byte[] BuildFieldKey(string field) => FieldPrefix.Concat(Encoding.UTF8.GetBytes(field));

        #endregion

        #region Field Vectors

        private static readonly byte[] FieldVectorKeyPrefix = Encoding.UTF8.GetBytes("K:");
        public static byte[] GetAllFieldVectorKeys() => FieldVectorKeyPrefix;
        public static ReadOnlySpan<byte> BuildFieldVectorKeyKey(string key) => FieldVectorKeyPrefix.Concat(Encoding.UTF8.GetBytes(key));

        private static readonly byte[] FieldVectorValuePrefix = Encoding.UTF8.GetBytes("V:");
        public static byte[] BuildFieldVectorValueKey(string key) => FieldVectorValuePrefix.Concat(Encoding.UTF8.GetBytes(key));

        #endregion

        #region Inverted Indices
        
        private static readonly byte[] InvertedIndexEntryPrefix = Encoding.UTF8.GetBytes("E:");

        public static byte[] BuildInvertedIndexEntryKey(string key) => InvertedIndexEntryPrefix.Concat(Encoding.UTF8.GetBytes(key));

        #endregion

        #region TokenSet

        private static readonly byte[] TokenSetWordPrefix = Encoding.UTF8.GetBytes("T:");
        public static byte[] BuildTokenSetWordKey(string word) => TokenSetWordPrefix.Concat(Encoding.UTF8.GetBytes(word));
        public static byte[] BuildAllTokenSetWordKeys() => TokenSetWordPrefix;

        #endregion

        public static byte[] Concat(this byte[] left, byte[] right)
        {
            var buffer = new byte[left.Length + right.Length];
            Buffer.BlockCopy(left, 0, buffer, 0, left.Length);
            Buffer.BlockCopy(right, 0, buffer, left.Length, right.Length);
            return buffer;
        }
    }
}