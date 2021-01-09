using System.Text.RegularExpressions;

namespace Lunr.Globalization.jp
{
	public sealed class JapaneseTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Japanese + "]+|[^" + WordCharacters.Japanese + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}