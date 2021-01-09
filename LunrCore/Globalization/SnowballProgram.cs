using System;

namespace Lunr.Globalization
{
	internal sealed class SnowballProgram
	{
		internal int bra;
		private string current;

		internal int cursor;
		internal int ket;
		internal int limit;
		internal int limit_backward;

		public void SetCurrent(string word)
		{
			current = word;
			cursor = 0;
			limit = word.Length;
			limit_backward = 0;
			bra = cursor;
			ket = limit;
		}

		public string GetCurrent()
		{
			var result = current;
			current = null!;
			return result;
		}

		public bool InGrouping(int[] s, int min, int max)
		{
			if (cursor < limit)
			{
				var ch = (int) current[cursor];
				if (ch <= max && ch >= min)
				{
					ch -= min;
					var r = s[ch >> 3] & (0X1 << (ch & 0X7));
					if (r == 0)
					{
						cursor++;
						return true;
					}
				}
			}

			return false;
		}

		public bool InGroupingBackwards(int[] s, int min, int max)
		{
			if (cursor > limit_backward)
			{
				var ch = (int) current[cursor - 1];
				if (ch <= max && ch >= min)
				{
					ch -= min;
					var r = s[ch >> 3] & (0X1 << (ch & 0X7));
					if (r == 0)
					{
						cursor--;
						return true;
					}
				}
			}

			return false;
		}

		public bool OutGrouping(int[] s, int min, int max)
		{
			if (cursor < limit)
			{
				var ch = (int) current[cursor];
				if (ch > max || ch < min)
				{
					cursor++;
					return true;
				}

				ch -= min;
				var r = s[ch >> 3] & (0X1 << (ch & 0X7));
				if (r != 0)
				{
					cursor++;
					return true;
				}
			}

			return false;
		}

		public bool OutGroupingBackwards(int[] s, int min, int max)
		{
			if (cursor > limit_backward)
			{
				var ch = (int) current[cursor - 1];
				if (ch > max || ch < min)
				{
					cursor--;
					return true;
				}

				ch -= min;
				var r = s[ch >> 3] & (0X1 << (ch & 0X7));
				if (r != 0)
				{
					cursor--;
					return true;
				}
			}

			return false;
		}

		public bool EqualsSegment(int s_size, string s)
		{
			if (limit - cursor < s_size)
			{
				return false;
			}

			for (var i = 0; i < s_size; i++)
			{
				if (current[cursor + i] != s[i])
				{
					return false;
				}
			}

			cursor += s_size;
			return true;
		}

		public bool EqualsSegmentBackwards(int s_size, string s)
		{
			if (cursor - limit_backward < s_size)
			{
				return false;
			}

			for (var i = 0; i < s_size; i++)
			{
				if (current[cursor - s_size + i] != s[i])
				{
					return false;
				}
			}

			cursor -= s_size;
			return true;
		}

		public int FindAmong(Among[] v, int v_size)
		{
			var i = 0;
			var j = v_size;
			var c = cursor;
			var l = limit;
			var common_i = 0;
			var common_j = 0;
			var first_key_inspected = false;

			while (true)
			{
				var k = i + ((j - i) >> 1);
				var diff = 0;
				var common = common_i < common_j ? common_i : common_j;
				var w = v[k];

				for (var i2 = common; i2 < w.Size; i2++)
				{
					if (c + common == l)
					{
						diff = -1;
						break;
					}

					diff = current[c + common] - w.StringArray[i2];
					if (diff != 0) /* !! */
					{
						break;
					}

					common++;
				}

				if (diff < 0)
				{
					j = k;
					common_j = common;
				}
				else
				{
					i = k;
					common_i = common;
				}

				if (j - i <= 1)
				{
					if (i > 0 || j == i || first_key_inspected)
					{
						break;
					}

					first_key_inspected = true;
				}
			}

			while (true)
			{
				var w = v[i];
				if (common_i >= w.Size)
				{
					cursor = c + w.Size;
					if (w.Method == null)
					{
						return w.Result;
					}

					var res = w.Method();
					cursor = c + w.Size;
					if (res)
					{
						return w.Result;
					}
				}

				i = w.Substring;
				if (i < 0)
				{
					return 0;
				}
			}
		}

		public int FindAmongBackwards(Among[] v, int v_size)
		{
			var i = 0;
			var j = v_size;
			var c = cursor;
			var lb = limit_backward;
			var common_i = 0;
			var common_j = 0;
			var first_key_inspected = false;

			while (true)
			{
				var k = i + ((j - i) >> 1);
				var diff = 0;
				var common = common_i < common_j ? common_i : common_j;
				var w = v[k];

				for (var i2 = w.Size - 1 - common; i2 >= 0; i2--)
				{
					if (c - common == lb)
					{
						diff = -1;
						break;
					}

					diff = current[c - 1 - common] - w.StringArray[i2];
					if (diff != 0) /* !! */
					{
						break;
					}

					common++;
				}

				if (diff < 0)
				{
					j = k;
					common_j = common;
				}
				else
				{
					i = k;
					common_i = common;
				}

				if (j - i <= 1)
				{
					if (i > 0 || j == i || first_key_inspected)
					{
						break;
					}

					first_key_inspected = true;
				}
			}

			while (true)
			{
				var w = v[i];
				if (common_i >= w.Size)
				{
					cursor = c - w.Size;
					if (w.Method == null)
					{
						return w.Result;
					}

					var res = w.Method();
					cursor = c - w.Size;
					if (res)
					{
						return w.Result;
					}
				}

				i = w.Substring;
				if (i < 0)
				{
					return 0;
				}
			}
		}

		public int ReplaceSegment(int c_bra, int c_ket, string s)
		{
			var adjustment = s.Length - (c_ket - c_bra);
			var left = current.Substring(0, c_bra);
			var right = current.Substring(c_ket);

			current = left + s + right;
			limit += adjustment;
			if (cursor >= c_ket)
			{
				cursor += adjustment;
			}
			else if (cursor > c_bra)
			{
				cursor = c_bra;
			}

			return adjustment;
		}

		public void SliceCheck()
		{
			if (bra < 0 || bra > ket || ket > limit || limit > current?.Length)
			{
				throw new InvalidOperationException("faulty slice operation");
			}
		}

		public void SliceFrom(string s)
		{
			SliceCheck();
			ReplaceSegment(bra, ket, s);
		}

		public void SliceDelete()
		{
			SliceFrom("");
		}

		public void Insert(int c_bra, int c_ket, string s)
		{
			var adjustment = ReplaceSegment(c_bra, c_ket, s);
			if (c_bra <= bra)
			{
				bra += adjustment;
			}

			if (c_bra <= ket)
			{
				ket += adjustment;
			}
		}

		public string SliceTo()
		{
			SliceCheck();
			return current.Substring(bra, ket);
		}

		public bool EqualsValueBackwards(string s)
		{
			return EqualsSegmentBackwards(s.Length, s);
		}
	}
}