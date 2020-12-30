using System;
using System.IO;

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

        public void Write(string value)
        {
            _bw.Write(value.Length);
            foreach(var c in value)
                _bw.Write((byte) c);
        }
    }
}