using Lunr;
using Xunit;

namespace LunrCoreTests
{
    public class TrimmerTests
    {
        [Theory]
        [InlineData("hello", "hello")] // word
        [InlineData("hello.", "hello")] // full stop
        [InlineData("it's", "it's")] // inner apostrophe
        [InlineData("james'", "james")] // trailing apostrophe
        [InlineData("stop!'", "stop")] // exclamation mark
        [InlineData("first,'", "first")] // comma
        [InlineData("[tag]'", "tag")] // brackets
        public void CheckTrim(string str, string expected)
        {
            Assert.Equal(expected, new Trimmer().Trim(str));
        }
    }
}
