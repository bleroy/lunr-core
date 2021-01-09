using System.Text.RegularExpressions;

namespace Lunr.Globalization.vi
{
	public sealed class VietnameseTrimmer : TrimmerBase
	{
		private static readonly Regex Pattern = new Regex("(^[^" + WordCharacters.Vietnamese + "]+|[^" + WordCharacters.Vietnamese + "]+$)", RegexOptions.Compiled);
		public override string Trim(string s) => Pattern.Replace(s, "");
	}
}