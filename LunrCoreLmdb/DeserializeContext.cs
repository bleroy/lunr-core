using System;
using System.Buffers.Binary;
using System.Runtime.Serialization;
using System.Text;

namespace LunrCoreLmdb
{
    public sealed class DeserializeContext
    {
        public int Version { get; }
        
        public DeserializeContext(ref ReadOnlySpan<byte> buffer)
        {
            Version = ReadInt32(ref buffer);
            if(Version > SerializeContext.FormatVersion)
                throw new SerializationException("Tried to read an object with a version that is too new");
        }
         
        public int ReadInt32(ref ReadOnlySpan<byte> buffer)
        {
            var value = BitConverter.IsLittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(buffer) : BinaryPrimitives.ReadInt32BigEndian(buffer);
            buffer = buffer.Slice(sizeof(int));
            return value;
        }
        
        public double ReadDouble(ref ReadOnlySpan<byte> buffer)
        {
            var value = BitConverter.Int64BitsToDouble(BitConverter.IsLittleEndian ? BinaryPrimitives.ReadInt64LittleEndian(buffer) : BinaryPrimitives.ReadInt64BigEndian(buffer));
            buffer = buffer.Slice(sizeof(long));
            return value;
        }

        public char ReadChar(ref ReadOnlySpan<byte> buffer)
        {
            var value = (char) buffer[0];
            buffer = buffer.Slice(1);
            return value;
        }
        
        public string ReadString(ref ReadOnlySpan<byte> buffer)
        {
            var length = ReadInt32(ref buffer);
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                var c =  ReadChar(ref buffer);
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}