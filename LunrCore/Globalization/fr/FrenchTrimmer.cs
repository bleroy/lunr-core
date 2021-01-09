using System.Text.RegularExpressions;

namespace Lunr.Globalization.fr
{
	public sealed class FrenchTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.French + "]+|[^" + WordCharacters.French + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}