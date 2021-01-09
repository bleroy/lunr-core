using System.Text.RegularExpressions;

namespace Lunr.Globalization.de
{
	public sealed class GermanTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.German + "]+|[^" + WordCharacters.German + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}