// Ported from: https://github.com/MihaiValentin/lunr-languages/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lunr.Multi
{
    public readonly struct Among : IEquatable<Among>
    {
        private readonly object s_size;
        private readonly char[] s;
        private readonly int result;
        private readonly string? method;
        private readonly int substring_i;

        public Among(string s, int substring_i, int result, string method = default!)
        {
            if (s == null)
                throw new ArgumentException($"Bad Among initialization: s:{s}, substring_i: {substring_i}, result: {result}");
            this.s_size = s.Length;
            this.s = s.ToCharArray();
            this.substring_i = substring_i;
            this.result = result;
            this.method = method;
        }

        public bool Equals(Among other)
        {
            return s_size.Equals(other.s_size) && 
                   s.SequenceEqual(other.s) && 
                   result == other.result &&
                   method == other.method && 
                   substring_i == other.substring_i;
        }

        public override bool Equals(object? obj)
        {
            return obj is Among other && Equals(other);
        }

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

        public static bool operator ==(Among left, Among right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Among left, Among right)
        {
            return !left.Equals(right);
        }

        private sealed class AmongEqualityComparer : IEqualityComparer<Among>
        {
            public bool Equals(Among x, Among y)
            {
                return x.s_size.Equals(y.s_size) && x.s.Equals(y.s) && x.result == y.result && x.method == y.method && x.substring_i == y.substring_i;
            }

            public int GetHashCode(Among obj)
            {
                unchecked
                {
                    var hashCode = obj.s_size.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.s.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.result;
                    hashCode = (hashCode * 397) ^ (obj.method != null ? obj.method.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ obj.substring_i;
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<Among> AmongComparer { get; } = new AmongEqualityComparer();
    }
}