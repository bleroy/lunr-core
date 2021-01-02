using System.Text.RegularExpressions;

namespace Lunr.Globalization.de
{
    public sealed class GermanTrimmer : TrimmerBase
    {
        private static readonly Regex StartRegex;
        private static readonly Regex EndRegex;

        static GermanTrimmer()
        {
            StartRegex = new Regex("^[^" + WordCharacters.German + "]+", RegexOptions.Compiled);
            EndRegex = new Regex("[^" + WordCharacters.German + "]+$", RegexOptions.Compiled);
        }

        public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
    }
}