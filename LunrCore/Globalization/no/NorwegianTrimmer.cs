using System.Text.RegularExpressions;

namespace Lunr.Globalization.no
{
	public sealed class NorwegianTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex = new Regex("^[^" + WordCharacters.Norwegian + "]+", RegexOptions.Compiled);
		private static readonly Regex EndRegex = new Regex("[^" + WordCharacters.Norwegian + "]+$", RegexOptions.Compiled);
		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}