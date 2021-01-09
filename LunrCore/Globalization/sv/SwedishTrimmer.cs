using System.Text.RegularExpressions;

namespace Lunr.Globalization.sv
{
	public sealed class SwedishTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Swedish + "]+|[^" + WordCharacters.Swedish + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}