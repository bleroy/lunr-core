using System.Text.RegularExpressions;

namespace Lunr.Globalization.vi
{
	public sealed class VietnameseTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static VietnameseTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Vietnamese + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Vietnamese + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
	}
}