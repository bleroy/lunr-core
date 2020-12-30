// Ported from: https://github.com/MihaiValentin/lunr-languages/blob/master/LICENSE

namespace Lunr.Multi
{
    internal sealed class SnowballProgram
    {
        private string current;
        private int cursor;
        private int limit;
        private int limit_backward;
        private int bra;
        private int ket;

        public void SetCurrent(string word)
        {
            current = word;
            this.cursor = 0;
            this.limit = word.Length;
            this.limit_backward = 0;
            this.bra = this.cursor;
            this.ket = this.limit;
        }

        public string GetCurrent()
        {
            var result = current;
            current = null;
            return result;
        }

        public bool in_grouping(string s, int min, int max) {
            if (this.cursor < this.limit) {
                var ch = (int) current[this.cursor];
                if (ch <= max && ch >= min) {
                    ch -= min;
                    int r = s[ch >> 3] & (0X1 << (ch & 0X7));
                    if (r == 0) {
                        this.cursor++;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool in_grouping_b(string s, int min, int max) {
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
    }
}