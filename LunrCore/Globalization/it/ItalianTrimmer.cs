using System.Text.RegularExpressions;

namespace Lunr.Globalization.it
{
	public sealed class ItalianTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static ItalianTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Italian + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Italian + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
	}
}