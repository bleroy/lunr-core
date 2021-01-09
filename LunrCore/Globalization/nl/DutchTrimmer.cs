using System.Text.RegularExpressions;

namespace Lunr.Globalization.nl
{
	public sealed class DutchTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Dutch + "]+|[^" + WordCharacters.Dutch + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}