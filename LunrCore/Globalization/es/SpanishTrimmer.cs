using System.Text.RegularExpressions;

namespace Lunr.Globalization.es
{
	public sealed class SpanishTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Spanish + "]+|[^" + WordCharacters.Spanish + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}