using System.Text.RegularExpressions;

namespace Lunr.Globalization.ru
{
    public sealed class RussianTrimmer : TrimmerBase
    {
        private static readonly Regex StartRegex;
        private static readonly Regex EndRegex;

        static RussianTrimmer()
        {
            StartRegex = new Regex("^[^" + WordCharacters.Russian + "]+", RegexOptions.Compiled);
            EndRegex = new Regex("[^" + WordCharacters.Russian + "]+$", RegexOptions.Compiled);
        }

        public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
    }
}