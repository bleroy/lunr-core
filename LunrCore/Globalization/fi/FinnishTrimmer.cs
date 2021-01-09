using System.Text.RegularExpressions;

namespace Lunr.Globalization.fi
{
	public sealed class FinnishTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Finnish + "]+|[^" + WordCharacters.Finnish + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}