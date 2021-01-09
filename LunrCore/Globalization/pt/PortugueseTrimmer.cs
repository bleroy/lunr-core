using System.Text.RegularExpressions;

namespace Lunr.Globalization.pt
{
	public sealed class PortugueseTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex = new Regex("^[^" + WordCharacters.Portuguese + "]+", RegexOptions.Compiled);
		private static readonly Regex EndRegex = new Regex("[^" + WordCharacters.Portuguese + "]+$", RegexOptions.Compiled);
		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}