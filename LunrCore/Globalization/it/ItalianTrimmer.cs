using System.Text.RegularExpressions;

namespace Lunr.Globalization.it
{
	public sealed class ItalianTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Italian + "]+|[^" + WordCharacters.Italian + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}