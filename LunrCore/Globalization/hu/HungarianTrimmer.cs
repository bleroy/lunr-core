using System.Text.RegularExpressions;

namespace Lunr.Globalization.hu
{
	public sealed class HungarianTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Hungarian + "]+|[^" + WordCharacters.Hungarian + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}