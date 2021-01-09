using System.Text.RegularExpressions;

namespace Lunr.Globalization.nl
{
	public sealed class DutchTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static DutchTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Dutch + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Dutch + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}