using System.Text.RegularExpressions;

namespace Lunr.Globalization.ro
{
	public sealed class RomanianTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Romanian + "]+|[^" + WordCharacters.Romanian + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}