using System.Text.RegularExpressions;

namespace Lunr.Globalization.hu
{
	public sealed class HungarianTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static HungarianTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Hungarian + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Hungarian + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}