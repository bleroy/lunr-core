using System.Text.RegularExpressions;

namespace Lunr.Globalization.tr
{
	public sealed class TurkishTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Turkish + "]+|[^" + WordCharacters.Turkish + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}