using System;
using System.IO;

namespace LunrCoreLmdb
{
    public sealed class SerializeContext
    {
        public const ulong FormatVersion = 1UL;

        public readonly BinaryWriter bw;

        public SerializeContext(BinaryWriter bw, ulong version = FormatVersion)
        {
            this.bw = bw;
            if (Version > FormatVersion)
                throw new Exception("Tried to write an object with a version that is too new");
            Version = version;

            bw.Write(Version);
        }

        public ulong Version { get; }
    }
}