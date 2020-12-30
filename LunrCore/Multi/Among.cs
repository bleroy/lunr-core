// Ported from: https://github.com/MihaiValentin/lunr-languages/blob/master/LICENSE

using System;

namespace Lunr.Multi
{
    internal struct Among
    {
        private object s_size;
        private char[] s;
        private string result;
        private string method;
        private int substring_i;

        public Among(string s, int substring_i, string result, string method)
        {
            if ((string.IsNullOrEmpty(s)) || substring_i != 0 || result == null)
                throw new ArgumentException($"Bad Among initialization: s:{s}, substring_i: {substring_i}, result: {result}");
            this.s_size = s.Length;
            this.s = s.ToCharArray();
            this.substring_i = substring_i;
            this.result = result;
            this.method = method;
        }
    }
}