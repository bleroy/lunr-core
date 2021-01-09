using System.Collections.Generic;
using System.Threading.Tasks;
using Lunr;
using Xunit;

namespace LunrCoreTests.Globalization.it
{
    public class ItalianSearchTests
    {
        private readonly Document[] _documents =
        {
            new Document
            {
                { "id", "1" },
                { "title", "Italia" },
                { "body", "L'Italia (/iˈtalja/[9], ascolta[?·info]), ufficialmente Repubblica Italiana,[10] è una repubblica parlamentare situata nell'Europa meridionale, con una popolazione di 60,6 milioni di abitanti e Roma come capitale. Delimitata dall'arco alpino, confina a nord, da ovest a est, con Francia, Svizzera, Austria e Slovenia; il resto del territorio, circondato dai mari Ligure, Tirreno, Ionio e Adriatico, si protende nel mar Mediterraneo, occupando la penisola italiana e numerose isole (le maggiori sono Sicilia e Sardegna), per un totale di 301 340 km²[11]. Gli Stati della Città del Vaticano e di San Marino sono enclavi della Repubblica." }
            },
            new Document
            {
                { "id", "2" },
                { "title", "Suddivisioni amministrative" },
                { "body", "Gli enti territoriali che, in base all'articolo 114 della Costituzione costituiscono, assieme allo Stato, la Repubblica italiana sono:            le regioni (15 a statuto ordinario e 5 a statuto speciale); le città metropolitane (14); le province e i comuni (rispettivamente 93 e 7 999, dati ISTAT dell'anno 2016).[121] Nell'elenco che segue, per ciascuna regione è riportato lo stemma ufficiale e il nome del capoluogo. pronunziato" }
            },
        };

        [Theory]
        [InlineData("Italia*", 2)]
        [InlineData("assieme", 1)]
        [InlineData("pronunziarle", 1)]
        [InlineData("inexistent", 0)]
        public async Task FindTheWord(string word, int resultCount)
        {
            Index idx = await GetPlainIndex();
            IList<Result> results = await idx.Search(word).ToList();
            Assert.Equal(resultCount, results.Count);
        }
        
        private async Task<Index> GetPlainIndex()
        {
            return await Lunr.Globalization.it.Index.Build(async builder =>
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