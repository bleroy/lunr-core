using Lunr;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LunrCoreTests
{
    public class StemmerTests
    {
        [Fact]
        public async Task StemmerReducesWordsToTheirStem()
        {
            Dictionary<string, string> testData = JsonSerializer
                .Deserialize<Dictionary<string, string>>(
                    File.ReadAllText(Path.Combine("fixtures", "stemming_vocab.json")));

            foreach((string word, string expected) in testData)
            {
                string result = (await new EnglishStemmer()
                    .StemmerFunction
                    .BasicallyRun(word))
                    .Single();
                Assert.Equal(expected, result);
            }
        }
    }
}
