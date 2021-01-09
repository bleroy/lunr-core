using System.Text.RegularExpressions;

namespace Lunr.Globalization.jp
{
	public sealed class JapaneseTrimmer : TrimmerBase
	{
		private static readonly Regex StartRegex;
		private static readonly Regex EndRegex;

		static JapaneseTrimmer()
		{
			StartRegex = new Regex("^[^" + WordCharacters.Japanese + "]+", RegexOptions.Compiled);
			EndRegex = new Regex("[^" + WordCharacters.Japanese + "]+$", RegexOptions.Compiled);
		}

		public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, ""), "");
	}
}