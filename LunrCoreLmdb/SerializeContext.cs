using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Lunr;

namespace LunrCoreLmdb
{
    public sealed class SerializeContext
    {
        public const int FormatVersion = 1;

        private readonly BinaryWriter _bw;

        public SerializeContext(BinaryWriter bw, int version = FormatVersion)
        {
            _bw = bw;
            if (Version > FormatVersion)
                throw new Exception("Tried to write an object with a version that is too new");
            Version = version;
            _bw.Write(Version);
        }

        public int Version { get; }

        public void Write(int value) => _bw.Write(value);

        public void Write(double value) => _bw.Write(value);

        public bool Write(bool value)
        {
            _bw.Write(value);
            return value;
        }

        public void Write(byte[] value)
        {
            _bw.Write(value.Length);
            _bw.Write(value);
        }

        public void Write(string value)
        {
            _bw.Write(value.Length);
            foreach (var c in value)
                _bw.Write((byte) c);
        }

        internal static readonly Dictionary<Type, (Func<object, byte[]>, Func<byte[], object> deserialize)> KnownTypes;

        static SerializeContext()
        {
            KnownTypes = new Dictionary<Type, (Func<object, byte[]>, Func<byte[], object> deserialize)>
            {
                {typeof(short), (v => BitConverter.GetBytes((short) v), b => BitConverter.ToInt16(b, 0))},
                {typeof(int), (v => BitConverter.GetBytes((int) v), b => BitConverter.ToInt32(b, 0))},
                {typeof(long), (v => BitConverter.GetBytes((long) v), b => BitConverter.ToInt64(b, 0))},
                {typeof(ushort), (v => BitConverter.GetBytes((ushort) v), b => BitConverter.ToUInt16(b, 0))},
                {typeof(uint), (v => BitConverter.GetBytes((uint) v), b => BitConverter.ToUInt32(b, 0))},
                {typeof(ulong), (v => BitConverter.GetBytes((ulong) v), b => BitConverter.ToUInt64(b, 0))},
                {typeof(float), (v => BitConverter.GetBytes((int) v), b => BitConverter.ToSingle(b, 0))},
                {typeof(double), (v => BitConverter.GetBytes((int) v), b => BitConverter.ToDouble(b, 0))},
                {typeof(bool), (v => BitConverter.GetBytes((bool) v), b => BitConverter.ToBoolean(b, 0))},
                {typeof(char), (v => BitConverter.GetBytes((char) v), b => BitConverter.ToChar(b, 0))}
            };

            KnownTypes.Add(typeof(string), (v => Encoding.UTF8.GetBytes((string) v), b => Encoding.UTF8.GetString(b)));
            KnownTypes.Add(typeof(Slice), (v =>
            {
                var (start, length) = (Slice) v;
                return Encoding.UTF8.GetBytes($"{start}/{length}");
            }, b =>
            {
                var value = Encoding.UTF8.GetString(b);
                var tokens = value.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                return new Slice(Convert.ToInt32(tokens[0]), Convert.ToInt32(tokens[1]));
            }));
        }
        
        public static void AddKnownType<T>(Func<T, byte[]> typeToMemory, Func<byte[], T> memoryToType)
        {
            if(KnownTypes.ContainsKey(typeof(T)))
                KnownTypes.Remove(typeof(T));
            KnownTypes.Add(typeof(T), (v => typeToMemory((T) v), b => memoryToType(b)!));
        }
    }
}