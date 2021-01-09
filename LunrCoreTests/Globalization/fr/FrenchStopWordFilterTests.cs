using System.Threading.Tasks;
using Lunr;
using Lunr.Globalization.fr;
using Xunit;

namespace LunrCoreTests.Globalization.fr
{
    public class FrenchStopWordFilterTests
    {
        private readonly StopWordFilterBase filter = new FrenchStopWordFilter();

        [Fact]
        public async Task StopWordFilterFiltersStopWords()
        {
            string[] stopWords = new[] { "aurai", "elle", "leurs", "soyons", "êtes" };
            
            foreach (string word in stopWords)
            {
                Assert.True(filter.IsStopWord(word));
                Assert.Empty(await filter.FilterFunction.BasicallyRun(word));
            }
        }

        [Fact]
        public async Task StopWordFilterIgnoresNonStopWords()
        {
            string[] nonStopWords = new[] { "baleine", "bisou", "brindille", "câlin" };

            foreach (string word in nonStopWords)
            {
                Assert.False(filter.IsStopWord(word));
                Assert.Equal(new[] { word }, await filter.FilterFunction.BasicallyRun(word));
            }
        }
    }
}
