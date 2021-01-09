using System.Text.RegularExpressions;

namespace Lunr.Globalization.no
{
	public sealed class NorwegianTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Norwegian + "]+|[^" + WordCharacters.Norwegian + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}