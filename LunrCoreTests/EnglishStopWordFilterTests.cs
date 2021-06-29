using Lunr;
using System.Threading.Tasks;
using Xunit;

namespace LunrCoreTests
{
    public class EnglishStopWordFilterTests
    {
        private readonly StopWordFilterBase filter = new EnglishStopWordFilter();

        [Fact]
        public async Task EnglishStopWordFilterIgnoresCase()
        {
            string[] stopWords = new[] { "the", "The", "THE" };
            
            foreach (string word in stopWords)
            {
                Assert.True(filter.IsStopWord(word));
                Assert.Empty(await filter.FilterFunction.BasicallyRun(word));
            }
        }
    }
}
