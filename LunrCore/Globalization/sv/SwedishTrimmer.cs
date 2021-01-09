using System.Text.RegularExpressions;

namespace Lunr.Globalization.sv
{
	public sealed class SwedishTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex = new Regex("^[^" + WordCharacters.Swedish + "]+", RegexOptions.Compiled);
		private static readonly Regex EndRegex = new Regex("[^" + WordCharacters.Swedish + "]+$", RegexOptions.Compiled);
		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}