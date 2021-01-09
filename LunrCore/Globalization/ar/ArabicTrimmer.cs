using System.Text.RegularExpressions;

namespace Lunr.Globalization.ar
{
	public sealed class ArabicTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Arabic + "]+|[^" + WordCharacters.Arabic + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}