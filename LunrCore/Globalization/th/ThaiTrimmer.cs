using System.Text.RegularExpressions;

namespace Lunr.Globalization.th
{
	public sealed class ThaiTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Thai + "]+|[^" + WordCharacters.Thai + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}