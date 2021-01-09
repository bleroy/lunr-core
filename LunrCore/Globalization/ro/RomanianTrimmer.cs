using System.Text.RegularExpressions;

namespace Lunr.Globalization.ro
{
	public sealed class RomanianTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static RomanianTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Romanian + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Romanian + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
	}
}