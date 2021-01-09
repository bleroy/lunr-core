using System.Text.RegularExpressions;

namespace Lunr.Globalization.sv
{
	public sealed class SwedishTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static SwedishTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Swedish + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Swedish + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
	}
}