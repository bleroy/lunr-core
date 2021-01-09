using System.Collections.Generic;
using System.Threading.Tasks;
using Lunr;
using Xunit;
using Index = Lunr.Index;

namespace LunrCoreTests.Globalization.de
{
    public class GermanSearchTests
    {
        private readonly Document[] _documents = new[]
        {
            new Document
            {
                { "id", "1" },
                { "title", "Deutschland" },
                { "body", "An Deutschland grenzen neun Nachbarländer und naturräumlich im Norden die Gewässer der Nord- und Ostsee, im Süden das Bergland der Alpen. Es liegt in der gemäßigten Klimazone, zählt mit rund 80 Millionen Einwohnern zu den dicht besiedelten Flächenstaaten und gilt international als das Land mit der dritthöchsten Zahl von Einwanderern. aufeinanderfolgenden. auffassen." }
            },
            new Document
            {
                { "id", "2" },
                { "title", "Tourismus in Deutschland" },
                { "body", "Deutschland als Urlaubsziel verfügt über günstige Voraussetzungen: Gebirgslandschaften (Alpen und Mittelgebirge), See- und Flusslandschaften, die Küsten und Inseln der Nord- und Ostsee, zahlreiche Kulturdenkmäler und eine Vielzahl geschichtsträchtiger Städte sowie gut ausgebaute Infrastruktur. Vorteilhaft ist die zentrale Lage in Europa." }
            },
        };

        [Theory(Skip = "There is a bug in this stemmer")]
        [InlineData("Deutsch*", 2)]
        [InlineData("Urlaubsziel*", 1)]
        [InlineData("auffassung", 1)]
        [InlineData("inexistent", 0)]
        public async Task FindTheWord(string word, int resultCount)
        {
            Index idx = await GetPlainIndex();
            IList<Result> results = await idx.Search(word).ToList();
            Assert.Equal(resultCount, results.Count);
        }
        
        private async Task<Index> GetPlainIndex()
        {
            return await Lunr.Globalization.de.Index.Build(async builder =>
            {
                builder.ReferenceField = "id";

                builder
                    .AddField("title")
                    .AddField("body", boost: 10);

                foreach (Document doc in _documents)
                {
                    await builder.Add(doc);
                }
            });
        }
    }
}
