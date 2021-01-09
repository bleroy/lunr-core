using System.Text.RegularExpressions;

namespace Lunr.Globalization.pt
{
	public sealed class PortugueseTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Portuguese + "]+|[^" + WordCharacters.Portuguese + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}