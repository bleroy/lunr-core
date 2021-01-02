// Ported from: https://github.com/MihaiValentin/lunr-languages/blob/master/LICENSE

using System;
using System.Linq;

namespace Lunr.Globalization
{
    public readonly struct Among : IEquatable<Among>
    {
        public readonly int s_size;
        public readonly char[] s;
        public readonly int result;
        public readonly Func<bool>? method;
        public readonly int substring_i;

        public Among(string s, int substring_i, int result, Func<bool> method = default!)
        {
            if (s == null)
                throw new ArgumentException($"Bad Among initialization: s:{s}, substring_i: {substring_i}, result: {result}");
            this.s_size = s.Length;
            this.s = s.ToCharArray();
            this.substring_i = substring_i;
            this.result = result;
            this.method = method;
        }

        public bool Equals(Among other) =>
            s_size.Equals(other.s_size) && 
            s.SequenceEqual(other.s) && 
            result == other.result &&
            method == other.method && 
            substring_i == other.substring_i;

        public override bool Equals(object? obj) => obj is Among other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = s_size.GetHashCode();
                hashCode = (hashCode * 397) ^ s.GetHashCode();
                hashCode = (hashCode * 397) ^ result;
                hashCode = (hashCode * 397) ^ (method != null ? method.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ substring_i;
                return hashCode;
            }
        }

        public static bool operator ==(Among left, Among right) => left.Equals(right);

        public static bool operator !=(Among left, Among right) => !left.Equals(right);
    }
}