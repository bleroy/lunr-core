using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Lunr;

namespace LunrCoreLmdb
{
    public static class SerializationExtensions
    {
        #region Vector

        public static ReadOnlySpan<byte> Serialize(this Vector vector)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8);
            var context = new SerializeContext(bw);
                        
            context.Write(vector.Count);
            foreach (var value in vector.Save())
            {
                context.Write(value);
            }

            ms.Position = 0;
            return ms.GetBuffer();
        }

        public static Vector DeserializeFieldVector(this ReadOnlySpan<byte> buffer)
        {
            var context = new DeserializeContext(ref buffer);

            var count = context.ReadInt32(ref buffer);

            var values = new List<(int, double)>();
            for (var i = 0; i < count; i++)
            {
                var index = context.ReadDouble(ref buffer);
                var value = context.ReadDouble(ref buffer);
                values.Add(((int) index, value));
            }

            return new Vector(values.ToArray());
        }

        #endregion

        #region InvertedIndex

        public static ReadOnlySpan<byte> Serialize(this InvertedIndex invertedIndex)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8);
            var context = new SerializeContext(bw);

            var entries =
                invertedIndex.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                    .ToList();

            context.Write(entries.Count);
            foreach(var entry in entries)
            {
                context.Write(entry.Key);
                entry.Value.Serialize(context);
            }

            ms.Position = 0;
            return ms.GetBuffer();
        }
        
        public static InvertedIndex DeserializeInvertedIndex(this ReadOnlySpan<byte> buffer)
        {
            var invertedIndex = new InvertedIndex();
            var context = new DeserializeContext(ref buffer);
            var count = context.ReadInt32(ref buffer);
            for (var i = 0; i < count; i++)
            {
                var key = context.ReadString(ref buffer);
                var value = DeserializeInvertedIndexEntry(context, ref buffer);
                invertedIndex.Add(key, value);
            }

            return invertedIndex;
        }

        #endregion

        #region InvertedIndexEntry
        
        public static ReadOnlySpan<byte> Serialize(this InvertedIndexEntry invertedIndexEntry)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            var context = new SerializeContext(bw);

            Serialize(invertedIndexEntry, context);

            ms.Position = 0;
            return ms.GetBuffer();
        }

        public static void Serialize(this InvertedIndexEntry invertedIndexEntry, SerializeContext context)
        {
            context.Write(invertedIndexEntry.Index);
            context.Write(invertedIndexEntry.Count);

            foreach (KeyValuePair<string, FieldMatches> pair in invertedIndexEntry)
            {
                context.Write(pair.Key);
                context.Write(pair.Value.Count); // FieldMatches

                foreach (KeyValuePair<string, FieldMatchMetadata> fieldMatches in pair.Value)
                {
                    context.Write(fieldMatches.Key);
                    context.Write(fieldMatches.Value.Count); // FieldMatchMetadata

                    foreach (KeyValuePair<string, IList<object?>> fieldMatchMetadata in fieldMatches.Value)
                    {
                        context.Write(fieldMatchMetadata.Key);
                        context.Write(fieldMatchMetadata.Value.Count);

                        foreach (object? meta in fieldMatchMetadata.Value)
                        {
                            var type = meta?.GetType();
                            if (type == null)
                                continue;

                            if (!SerializeContext.KnownTypes.TryGetValue(type, out var serializer))
                                throw new SerializationException($"no serializer registered for '{type.Name}'");

                            if (context.Write(meta != null && type != null))
                            {
                                context.Write(type?.FullName!);
                                context.Write(serializer.Item1(meta ?? throw new InvalidOperationException("unexpected null value")));
                            }
                        }
                    }
                }
            }
        }

        public static InvertedIndexEntry DeserializeInvertedIndexEntry(this ReadOnlySpan<byte> buffer)
        {
            var context = new DeserializeContext(ref buffer);

            return DeserializeInvertedIndexEntry(context, ref buffer);
        }

        public static InvertedIndexEntry DeserializeInvertedIndexEntry(this DeserializeContext context, ref ReadOnlySpan<byte> buffer)
        {
            var entry = new InvertedIndexEntry();
            entry.Index = context.ReadInt32(ref buffer);
            var fieldMatchesCount = context.ReadInt32(ref buffer);

            for (var i = 0; i < fieldMatchesCount; i++)
            {
                var fieldMatches = new FieldMatches();

                var fieldMatchesKey = context.ReadString(ref buffer);
                var fieldMatchCount = context.ReadInt32(ref buffer);

                for (var j = 0; j < fieldMatchCount; j++)
                {
                    var fieldMatchMeta = new FieldMatchMetadata();

                    var fieldMatchMetaKey = context.ReadString(ref buffer);
                    var fieldMatchMetaCount = context.ReadInt32(ref buffer);

                    for (var k = 0; k < fieldMatchMetaCount; k++)
                    {
                        var fieldMatchMetaValueKey = context.ReadString(ref buffer);
                        var fieldMatchMetaValueCount = context.ReadInt32(ref buffer);

                        var meta = new List<object?>(fieldMatchMetaValueCount);

                        for (var l = 0; l < fieldMatchMetaValueCount; l++)
                        {
                            if (context.ReadBoolean(ref buffer))
                            {
                                var typeName = context.ReadString(ref buffer);
                                var type = Type.GetType(typeName);
                                if(type == null)
                                    throw new SerializationException($"no serializer registered for '{typeName}'");

                                if (!SerializeContext.KnownTypes.TryGetValue(type, out var serializer))
                                    throw new SerializationException($"no serializer registered for '{type.Name}'");

                                var data = context.ReadBytes(ref buffer);
                                var deserialized = serializer.Item2(data.ToArray());
                                meta.Add(deserialized);
                            }
                            else
                            {
                                meta.Add(null);
                            }
                        }
                        
                        fieldMatchMeta.Add(fieldMatchMetaValueKey, meta);
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
            var bw = new BinaryWriter(ms, Encoding.UTF8);
            var context = new SerializeContext(bw);

            tokenSet.Serialize(context);

            ms.Position = 0;
            return ms.GetBuffer();  
        }

        public static void Serialize(this TokenSet tokenSet, SerializeContext context)
        {
            var words = tokenSet.ToEnumeration().ToList();
            context.Write(words.Count);
            foreach (var word in words)
                context.Write(word);
        }

        public static TokenSet DeserializeTokenSet(this ReadOnlySpan<byte> buffer) => new DeserializeContext(ref buffer).DeserializeTokenSet(ref buffer);

        public static TokenSet DeserializeTokenSet(this DeserializeContext context, ref ReadOnlySpan<byte> buffer)
        {
            var builder = new TokenSet.Builder();
            var count = context.ReadInt32(ref buffer);
            for (var i = 0; i < count; i++)
            {
                var word = context.ReadString(ref buffer);
                builder.Insert(word);
            }
            return builder.Root;
        }

        #endregion
    }
}
