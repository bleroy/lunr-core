using System.Text.RegularExpressions;

namespace Lunr.Globalization.es
{
	public sealed class SpanishTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex = new Regex("^[^" + WordCharacters.Spanish + "]+", RegexOptions.Compiled);
		private static readonly Regex EndRegex = new Regex("[^" + WordCharacters.Spanish + "]+$", RegexOptions.Compiled);
		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}