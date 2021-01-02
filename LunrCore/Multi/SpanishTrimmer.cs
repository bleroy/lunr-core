using System.Text.RegularExpressions;

namespace Lunr.Multi
{
    public sealed class SpanishTrimmer : TrimmerBase
    {
        private static readonly Regex StartRegex;
        private static readonly Regex EndRegex;

        static SpanishTrimmer()
        {
            StartRegex = new Regex("^[^" + WordCharacters.Spanish + "]+", RegexOptions.Compiled);
            EndRegex = new Regex("[^" + WordCharacters.Spanish + "]+$", RegexOptions.Compiled);
        }

        public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
    }
}