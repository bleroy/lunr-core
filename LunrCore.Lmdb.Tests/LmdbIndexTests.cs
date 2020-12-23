using System;
using System.Linq;
using Xunit;

namespace LunrCore.Lmdb.Tests
{
    public class LmdbIndexTests
    {
        [Fact]
        public void Can_add_and_retrieve_fields()
        {
            var index = new LmdbIndex($"{Guid.NewGuid()}");

            try
            {
                var addedField = index.AddField("Field");
                Assert.True(addedField);

                var fields = index.GetFields();
                Assert.NotNull(fields);
                Assert.Equal("Field", fields.Single());
            }
            finally
            {
                index.Destroy();
                index.Dispose();
            }
        }
    }
}
