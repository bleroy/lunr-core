using System;
using System.Linq;

namespace Lunr.Globalization
{
	public readonly struct Among : IEquatable<Among>
	{
		public int Size { get; }
		public char[] StringArray { get; }
		public int Result { get; }
		public Func<bool>? Method { get; }
		public int Substring { get; }

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
			return (Size, StringArray, Result, Method, Substring).GetHashCode();
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