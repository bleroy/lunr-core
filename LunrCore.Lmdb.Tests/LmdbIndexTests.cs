using System;
using System.Linq;
using Xunit;

namespace LunrCore.Lmdb.Tests
{
    public class LmdbIndexTests
    {
        [Theory]
        [InlineData("Field")]
        public void Can_add_and_retrieve_fields(string field)
        {
            var index = new LmdbIndex($"{Guid.NewGuid()}");

            try
            {
                var addedField = index.AddField(field);
                Assert.True(addedField);

                var fields = index.GetFields();
                Assert.NotNull(fields);
                Assert.Equal(field, fields.Single());
            }
            finally
            {
                index.Destroy();
                index.Dispose();
            }
        }
    }
}
