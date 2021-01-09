// Ported from: https://github.com/MihaiValentin/lunr-languages/blob/master/LICENSE

using System;
using System.Linq;

namespace Lunr.Globalization
{
	public readonly struct Among : IEquatable<Among>
	{
		public readonly int Size;
		public readonly char[] StringArray;
		public readonly int Result;
		public readonly Func<bool>? Method;
		public readonly int Substring;

		public Among(string s, int substring, int result, Func<bool> method = default!)
		{
			if (s == null)
			{
				throw new ArgumentNullException(nameof(s));
			}

			Size = s.Length;
			StringArray = s.ToCharArray();
			Substring = substring;
			Result = result;
			Method = method;
		}

		public bool Equals(Among other)
		{
			return Size == other.Size &&
			       StringArray.SequenceEqual(other.StringArray) &&
			       Result == other.Result &&
			       Method == other.Method &&
			       Substring == other.Substring;
		}

		public override bool Equals(object? obj)
		{
			return obj is Among other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Size.GetHashCode();
				hashCode = (hashCode * 397) ^ StringArray.GetHashCode();
				hashCode = (hashCode * 397) ^ Result;
				hashCode = (hashCode * 397) ^ (Method != null ? Method.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Substring;
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
	}
}