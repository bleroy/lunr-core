// Ported from: https://github.com/MihaiValentin/lunr-languages/blob/master/LICENSE

using System;

namespace Lunr.Multi
{
    internal sealed class SnowballProgram
    {
        private string current;

        internal int cursor;
        internal int limit;
        internal int limit_backward;
        internal int bra;
        internal int ket;

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

        public bool in_grouping(int[] s, int min, int max) {
            if (cursor < limit) {
                var ch = (int) current[cursor];
                if (ch <= max && ch >= min) {
                    ch -= min;
                    int r = s[ch >> 3] & (0X1 << (ch & 0X7));
                    if (r == 0) {
                        cursor++;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool in_grouping_b(int[] s, int min, int max) {
            if (this.cursor > this.limit_backward) {

                var ch = (int) current[this.cursor - 1];
                if (ch <= max && ch >= min) {
                    ch -= min;
                    var r = s[ch >> 3] & (0X1 << (ch & 0X7));
                    if (r == 0) {
                        this.cursor--;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool out_grouping(int[] s, int min, int max)
        {
            if (this.cursor < this.limit) {
                var ch = (int) current[this.cursor];
                if (ch > max || ch < min) {
                    this.cursor++;
                    return true;
                }
                ch -= min;
                var r = (s[ch >> 3] & (0X1 << (ch & 0X7)));
                if (r != 0) {
                    this.cursor++;
                    return true;
                }
            }
            return false;
        }

        public bool out_grouping_b(int[] s, int min, int max) {
            if (this.cursor > this.limit_backward) {
                var ch = (int) current[this.cursor - 1];
                if (ch > max || ch < min) {
                    this.cursor--;
                    return true;
                }
                ch -= min;
                var r = (s[ch >> 3] & (0X1 << (ch & 0X7)));
                if (r != 0) {
                    this.cursor--;
                    return true;
                }
            }
            return false;
        }

        public bool eq_s(int s_size, string s) {
            if (this.limit - this.cursor < s_size)
                return false;
            for (var i = 0; i < s_size; i++)
                if (current[this.cursor + i] != s[i])
                    return false;
            this.cursor += s_size;
            return true;
        }
        
        public bool eq_s_b(int s_size, string s) {
            if (this.cursor - this.limit_backward < s_size)
                return false;
            for (var i = 0; i < s_size; i++)
                if (current[this.cursor - s_size + i] != s[i])
                    return false;
            this.cursor -= s_size;
            return true;
        }

        public int find_among(Among[] v, int v_size)
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

                for (var i2 = common; i2 < w.s_size; i2++)
                {
                    if (c + common == l)
                    {
                        diff = -1;
                        break;
                    }
                    diff = current[c + common] - w.s[i2];
                    if (diff != 0) /* !! */
                        break;
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
                        break;
                    first_key_inspected = true;
                }
            }
            while (true)
            {
                var w = v[i];
                if (common_i >= w.s_size)
                {
                    cursor = c + w.s_size;
                    if (w.method == null)
                        return w.result;
                    var res = w.method();
                    cursor = c + w.s_size;
                    if (res)
                        return w.result;
                }
                i = w.substring_i;
                if (i < 0)
                    return 0;
            }
        }
        
        public int find_among_b(Among[] v, int v_size)
        {
            var i = 0;
            var j = v_size;
            var c = cursor;
            var lb = this.limit_backward;
            var common_i = 0;
            var common_j = 0;
            var first_key_inspected = false;

            while (true)
            {
                var k = i + ((j - i) >> 1);
                var diff = 0;
                var common = common_i < common_j ? common_i : common_j;
                var w = v[k];

                for (var i2 = w.s_size - 1 - common; i2 >= 0; i2--)
                {
                    if (c - common == lb)
                    {
                        diff = -1;
                        break;
                    }
                    diff = current[(c - 1 - common)] - w.s[i2];
                    if (diff != 0) /* !! */
                        break;
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
                        break;
                    first_key_inspected = true;
                }
            }
            while (true)
            {
                var w = v[i];
                if (common_i >= w.s_size)
                {
                    this.cursor = c - w.s_size;
                    if (w.method == null)
                        return w.result;
                    var res = w.method();
                    this.cursor = c - w.s_size;
                    if (res)
                        return w.result;
                }
                i = w.substring_i;
                if (i < 0)
                    return 0;
            }
        }
        
        public int replace_s(int c_bra, int c_ket, string s)
        {
            var adjustment = s.Length - (c_ket - c_bra);
            var left = current.Substring(0, c_bra);
            var right = current.Substring(c_ket);

            current = left + s + right;
            limit += adjustment;
            if (cursor >= c_ket)
                cursor += adjustment;
            else if (cursor > c_bra)
                cursor = c_bra;
            return adjustment;
        }

        public void slice_check()
        {
            if (bra < 0 || bra > ket || ket > limit|| limit > current?.Length)
                throw new InvalidOperationException("faulty slice operation");
        }

        public void slice_from(string s)
        {
            slice_check();
            replace_s(bra, ket, s);
        }

        public void slice_del()
        {
            slice_from(string.Empty);
        }

        public void insert(int c_bra, int c_ket, string s)
        {
            var adjustment = replace_s(c_bra, c_ket, s);
            if (c_bra <= bra)
                bra += adjustment;
            if (c_bra <= ket)
                ket += adjustment;
        }

        public string? slice_to()
        {
            slice_check();
            return current?.Substring(bra, ket);
        }

        public bool eq_v_b(string s) => eq_s_b(s.Length, s);
    }
}