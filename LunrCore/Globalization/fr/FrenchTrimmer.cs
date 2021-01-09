using System.Text.RegularExpressions;

namespace Lunr.Globalization.fr
{
	public sealed class FrenchTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex = new Regex("^[^" + WordCharacters.French + "]+", RegexOptions.Compiled);
		private static readonly Regex EndRegex = new Regex("[^" + WordCharacters.French + "]+$", RegexOptions.Compiled);
		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}