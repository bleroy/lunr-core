﻿using System.Text.RegularExpressions;

namespace Lunr.Multi
{
    public sealed class PortugueseTrimmer : TrimmerBase
    {
        private static readonly Regex StartRegex;
        private static readonly Regex EndRegex;

        static PortugueseTrimmer()
        {
            StartRegex = new Regex("^[^" + WordCharacters.Portuguese + "]+", RegexOptions.Compiled);
            EndRegex = new Regex("[^" + WordCharacters.Portuguese + "]+$", RegexOptions.Compiled);
        }

        public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
    }
}