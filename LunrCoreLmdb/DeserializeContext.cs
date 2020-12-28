using System;
using System.IO;

namespace LunrCore.Lmdb
{
    public sealed class DeserializeContext
    {
        public readonly BinaryReader br;

        public DeserializeContext(BinaryReader br)
        {
            this.br = br;

            Version = br.ReadUInt64();

            if (Version > SerializeContext.FormatVersion)
                throw new Exception("Tried to read an object with a version that is too new");
        }

        public ulong Version { get; }
    }
}