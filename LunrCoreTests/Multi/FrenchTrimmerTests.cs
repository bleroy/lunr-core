using Lunr.Multi;
using Xunit;

namespace LunrCoreTests.Multi
{
    public class FrenchTrimmerTests
    {
        [Theory]
        [InlineData("d’Amritsar", "d’Amritsar")] // word
        [InlineData("français.", "français")] // full stop
        [InlineData("l'accès", "l'accès")] // inner apostrophe
        [InlineData("Chloë'", "Chloë")] // trailing apostrophe
        [InlineData("C’est!'", "C’est")] // exclamation mark
        [InlineData("L’âge,'", "L’âge")] // comma
        [InlineData("[nationalité]'", "nationalité")] // brackets
        public void CheckTrim(string str, string expected)
        {
            Assert.Equal(expected, new FrenchTrimmer().Trim(str));
        }
    }
}