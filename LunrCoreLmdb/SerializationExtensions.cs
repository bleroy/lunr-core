using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lunr;

namespace LunrCoreLmdb
{
    public static class SerializationExtensions
    {
        #region Vector

        public static ReadOnlySpan<byte> Serialize(this Vector vector)
        {
            var ms = new MemoryStream();
            var bw =new BinaryWriter(ms);
            var context = new SerializeContext(bw);

            var values = vector.Save().ToList();
            context.bw.Write(values.Count);
            foreach (var value in values)
            {
                context.bw.Write(value);
            }

            ms.Position = 0;
            return ms.GetBuffer();
        }

        public static Vector DeserializeFieldVector(this ReadOnlySpan<byte> buffer)
        {
            unsafe
            {
                fixed(byte* buf = &buffer.GetPinnableReference())
                {
                    var ms = new UnmanagedMemoryStream(buf, buffer.Length);
                    var br = new BinaryReader(ms);
                    var context = new DeserializeContext(br);

                    var count = context.br.ReadInt32();
                    var values = new List<(int, double)>();
                    for (var i = 0; i < count; i++)
                    {
                        var index = context.br.ReadDouble();
                        var value = context.br.ReadDouble();
                        values.Add(((int) index, value));
                    }

                    return new Vector(values.ToArray());
                }
            }
        }

        #endregion

        #region InvertedIndex

        public static ReadOnlySpan<byte> Serialize(this InvertedIndex invertedIndex)
        {
            var ms = GetSerializationContext(out var context);

            var entries =
                invertedIndex.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                    .ToList();

            context.bw.Write(entries.Count);
            foreach(var entry in entries)
            {
                context.bw.Write(entry.Key);
                entry.Value.Serialize(context);
            }

            ms.Position = 0;
            return ms.GetBuffer();
        }
        
        public static InvertedIndex DeserializeInvertedIndex(this ReadOnlySpan<byte> buffer)
        {
            var invertedIndex = new InvertedIndex();

            unsafe
            {
                fixed (byte* buf = &buffer.GetPinnableReference())
                {
                    var ms = new UnmanagedMemoryStream(buf, buffer.Length);
                    var br = new BinaryReader(ms);
                    var context = new DeserializeContext(br);

                    var count = context.br.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var key = context.br.ReadString();
                        var value = DeserializeInvertedIndexEntry(context);
                        invertedIndex.Add(key, value);
                    }
                }
            }

            return invertedIndex;
        }

        #endregion

        #region InvertedIndexEntry
        
        public static ReadOnlySpan<byte> Serialize(this InvertedIndexEntry invertedIndexEntry)
        {
            var ms = GetSerializationContext(out var context);

            Serialize(invertedIndexEntry, context);

            ms.Position = 0;
            return ms.GetBuffer();
        }

        public static void Serialize(this InvertedIndexEntry invertedIndexEntry, SerializeContext context)
        {
            context.bw.Write(invertedIndexEntry.Index);
            context.bw.Write(invertedIndexEntry.Count);

            foreach (KeyValuePair<string, FieldMatches> pair in invertedIndexEntry)
            {
                context.bw.Write(pair.Key);
                context.bw.Write(pair.Value.Count); // FieldMatches

                foreach (KeyValuePair<string, FieldMatchMetadata> fieldMatches in pair.Value)
                {
                    context.bw.Write(fieldMatches.Key);
                    context.bw.Write(fieldMatches.Value.Count); // FieldMatchMetadata

                    foreach (KeyValuePair<string, IList<object?>> fieldMatchMetadata in fieldMatches.Value)
                    {
                        context.bw.Write(fieldMatchMetadata.Key);
                        context.bw.Write(fieldMatchMetadata.Value.Count);

                        // ??
                    }
                }
            }
        }

        public static InvertedIndexEntry DeserializeInvertedIndexEntry(this ReadOnlySpan<byte> buffer)
        {
            unsafe
            {
                fixed(byte* buf = &buffer.GetPinnableReference())
                {
                    var ms = new UnmanagedMemoryStream(buf, buffer.Length);
                    var br = new BinaryReader(ms);
                    var context = new DeserializeContext(br);

                    return DeserializeInvertedIndexEntry(context);
                }
            }
        }

        public static InvertedIndexEntry DeserializeInvertedIndexEntry(this DeserializeContext context)
        {
            var entry = new InvertedIndexEntry();
            entry.Index = context.br.ReadInt32(); // Index
            var fieldMatchesCount = context.br.ReadInt32(); // Count

            for (var i = 0; i < fieldMatchesCount; i++)
            {
                var fieldMatches = new FieldMatches();

                var fieldMatchesKey = context.br.ReadString();
                var fieldMatchCount = context.br.ReadInt32();

                for (var j = 0; j < fieldMatchCount; j++)
                {
                    var fieldMatchMeta = new FieldMatchMetadata();

                    var fieldMatchMetaKey = context.br.ReadString();
                    var fieldMatchMetaCount = context.br.ReadInt32();

                    for (var k = 0; k < fieldMatchMetaCount; k++)
                    {
                        var fieldMatchMetaValueKey = context.br.ReadString();
                        fieldMatchMeta.Add(fieldMatchMetaValueKey, new List<object?>());
                    }

                    fieldMatches.Add(fieldMatchMetaKey, fieldMatchMeta);
                }

                entry.Add(fieldMatchesKey, fieldMatches);
            }

            return entry;
        }

        #endregion

        #region TokenSet

        public static ReadOnlySpan<byte> Serialize(this TokenSet tokenSet)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            var context = new SerializeContext(bw);

            tokenSet.Serialize(context);

            ms.Position = 0;
            return ms.GetBuffer();  
        }

        public static void Serialize(this TokenSet tokenSet, SerializeContext context)
        {
            var words = tokenSet.ToEnumeration().ToList();
            context.bw.Write(words.Count);
            foreach (var word in words)
            {
                context.bw.Write(word);
            }
        }

        public static TokenSet DeserializeTokenSet(this ReadOnlySpan<byte> buffer)
        {
            unsafe
            {
                fixed(byte* buf = &buffer.GetPinnableReference())
                {
                    var ms = new UnmanagedMemoryStream(buf, buffer.Length);
                    var br = new BinaryReader(ms);
                    var context = new DeserializeContext(br);
                    return context.DeserializeTokenSet();
                }
            }
        }

        public static TokenSet DeserializeTokenSet(this DeserializeContext context)
        {
            var builder = new TokenSet.Builder();
            var count = context.br.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var word = context.br.ReadString();
                builder.Insert(word);
            }
            return builder.Root;
        }

        #endregion

        private static MemoryStream GetSerializationContext(out SerializeContext context)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            context = new SerializeContext(bw);
            return ms;
        }
    }
}
