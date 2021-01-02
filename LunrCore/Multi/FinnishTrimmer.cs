using System.Text.RegularExpressions;

namespace Lunr.Multi
{
    public sealed class FinnishTrimmer : TrimmerBase
    {
        private static readonly Regex StartRegex;
        private static readonly Regex EndRegex;

        static FinnishTrimmer()
        {
            StartRegex = new Regex("^[^" + WordCharacters.Finnish + "]+", RegexOptions.Compiled);
            EndRegex = new Regex("[^" + WordCharacters.Finnish + "]+$", RegexOptions.Compiled);
        }

        public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
    }
}