using Lunr.Serialization;
using System;
using System.Text.Json.Serialization;

namespace Lunr
{
    /// <summary>
    /// References a slice of text.
    /// </summary>
    [JsonConverter(typeof(SliceConverter))]
    public class Slice
    {
        public Slice(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} should be positive.");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"{nameof(length)} should be positive.");
            }
            Start = start;
            Length = length;
        }

        /// <summary>
        /// The start index of the slice.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// The length of the slice.
        /// </summary>
        public int Length { get; }

        public void Deconstruct(out int start, out int length) => (start, length) = (Start, Length);

        public override bool Equals(object? obj)
        {
            return obj is Slice otherSlice && Start == otherSlice.Start && Length == otherSlice.Length;
        }

        public override int GetHashCode() => (Start, Length).GetHashCode();
    }
}
