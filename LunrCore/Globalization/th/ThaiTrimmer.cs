using System.Text.RegularExpressions;

namespace Lunr.Globalization.th
{
	public sealed class ThaiTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static ThaiTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Thai + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Thai + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}