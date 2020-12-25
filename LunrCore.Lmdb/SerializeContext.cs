﻿using System;
using System.IO;

namespace LunrCore.Lmdb
{
    public sealed class SerializeContext
    {
        public const ulong FormatVersion = 1UL;

        public readonly BinaryWriter bw;

        public SerializeContext(BinaryWriter bw, ulong version = FormatVersion)
        {
            this.bw = bw;
            if (Version > FormatVersion)
                throw new Exception("Tried to write and object with a version that is too new");
            Version = version;

            bw.Write(Version);
        }

        public ulong Version { get; }
    }
}