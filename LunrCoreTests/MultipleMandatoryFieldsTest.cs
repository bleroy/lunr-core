using Lunr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LunrCoreTests
{
    public class MultipleMandatoryFieldsTest
    {
        [Fact]
        public async Task SearchShouldNotThrowExceptionWhenNoResult()
        {
            var index = Lunr.Index.Build(
                async builder =>
                {
                    builder
                        .AddField("ref")
                        .AddField("lastname", 3)
                        .AddField("firstname", 2);

                    builder.ReferenceField = "ref";

                    await builder.Add(new Lunr.Document()
                    {
                        { "ref", "0001" },
                        { "lastname", "Wonderland" },
                        { "firstname", "Alice" }
                    });

                    await builder.Add(new Lunr.Document()
                    {
                        { "ref", "0002" },
                        { "lastname", "Sponge" },
                        { "firstname", "Bob" }
                    });
                }
            ).Result;

            bool caughtException = false;

            try
            {
                var results = index.Search("+Alice +abc");
                int count = 0;
                await foreach (var result in results)
                {
                    count += 1;
                }
                Assert.Equal(0, count);
            }
            catch (AggregateException)
            {
                caughtException = true;
            }

            Assert.False(caughtException);
        }
    }
}
