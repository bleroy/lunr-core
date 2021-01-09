using System;

namespace Lunr.Globalization.vi
{
	public sealed class VietnameseStopWordFilter : StopWordFilterBase
	{
		private const string Data = @"là cái nhưng mà";

		private static readonly ISet<string> WordList =
			new Set<string>(Data.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries));

		protected override ISet<string> StopWords => WordList;
	}
}