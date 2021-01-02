using System.Text.RegularExpressions;

namespace Lunr.Globalization.fr
{
    public sealed class FrenchTrimmer : TrimmerBase
    {
        private static readonly Regex StartRegex;
        private static readonly Regex EndRegex;

        static FrenchTrimmer()
        {
            StartRegex = new Regex("^[^" + WordCharacters.French + "]+", RegexOptions.Compiled);
            EndRegex = new Regex("[^" + WordCharacters.French + "]+$", RegexOptions.Compiled);
        }

        public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
    }
}