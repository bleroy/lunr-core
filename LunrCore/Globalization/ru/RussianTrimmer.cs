using System.Text.RegularExpressions;

namespace Lunr.Globalization.ru
{
	public sealed class RussianTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Russian + "]+|[^" + WordCharacters.Russian + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}