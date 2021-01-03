// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Lunr.Globalization.de;
using Xunit;

namespace LunrCoreTests.Globalization.de
{
    public class GermanStemmerTests
    {
        [Theory]
        [InlineData("auffassen", "auffass")]
        [InlineData("auffassung", "auffass")] // habr1 converts this to auffassUng, which then sieves out
        public void Stems_standard_suffixes(string word, string stemmed)
        {
            Assert.Equal(stemmed, new GermanStemmer().Stem(word));
        }
    }
}