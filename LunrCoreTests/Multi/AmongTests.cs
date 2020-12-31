using System;
using Lunr.Multi;
using Xunit;

namespace LunrCoreTests.Multi
{
    public class AmongTests
    {
        [Theory]
        [InlineData("col", -1, -1)]
        [InlineData("a", -1, 3)]
        [InlineData("eriez", 33, 2)]
        [InlineData("", -1, 4)]
        public void Can_construct_valid_data(string s, int substring_i, int result)
        {
            var one = new Among(s, substring_i, result);
            var two = new Among(s, substring_i, result);

            Assert.True(one.Equals(two));
        }

        [Theory]
        [InlineData(default, -1, -1)]
        public void Throws_on_invalid_data(string s, int substring_i, int result)
        {
            Assert.Throws<ArgumentException>(() => new Among(s, substring_i, result));
        }
    }
}
