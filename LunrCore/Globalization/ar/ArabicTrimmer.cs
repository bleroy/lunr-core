using System.Text.RegularExpressions;

namespace Lunr.Globalization.ar
{
	public sealed class ArabicTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static ArabicTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Arabic + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Arabic + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
	}
}