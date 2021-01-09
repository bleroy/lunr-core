using System.Text.RegularExpressions;

namespace Lunr.Globalization.da
{
	public sealed class DanishTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Danish + "]+|[^" + WordCharacters.Danish + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}