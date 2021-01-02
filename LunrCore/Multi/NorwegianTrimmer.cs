﻿using System.Text.RegularExpressions;

namespace Lunr.Multi
{
    public sealed class NorwegianTrimmer : TrimmerBase
    {
        private static readonly Regex StartRegex;
        private static readonly Regex EndRegex;

        static NorwegianTrimmer()
        {
            StartRegex = new Regex("^[^" + WordCharacters.Norwegian + "]+", RegexOptions.Compiled);
            EndRegex = new Regex("[^" + WordCharacters.Norwegian + "]+$", RegexOptions.Compiled);
        }

        public override string Trim(string s) => EndRegex.Replace(StartRegex.Replace(s, string.Empty), string.Empty);
    }
}