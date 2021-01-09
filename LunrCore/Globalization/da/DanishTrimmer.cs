using System.Text.RegularExpressions;

namespace Lunr.Globalization.da
{
	public sealed class DanishTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static DanishTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Danish + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Danish + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
	}
}