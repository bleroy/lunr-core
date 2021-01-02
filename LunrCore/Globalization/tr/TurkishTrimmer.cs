using System.Text.RegularExpressions;

namespace Lunr.Globalization.tr
{
    public sealed class TurkishTrimmer : TrimmerBase
    {
        private static readonly Regex StartRegex;
        private static readonly Regex EndRegex;

        static TurkishTrimmer()
        {
            StartRegex = new Regex("^[^" + WordCharacters.Turkish + "]+", RegexOptions.Compiled);
            EndRegex = new Regex("[^" + WordCharacters.Turkish + "]+$", RegexOptions.Compiled);
        }

        public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
    }
}