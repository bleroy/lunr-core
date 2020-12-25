using System;
using System.Linq;
using System.Threading;
using Lunr;
using Xunit;

namespace LunrCore.Lmdb.Tests
{
    public class LmdbIndexTests
    {
        [Fact]
        public void Can_persist_fields()
        {
            const string field = "Field";

            var index = new LmdbIndex($"{Guid.NewGuid()}");

            try
            {
                var addedField = index.AddField(field);
                Assert.True(addedField);

                var fields = index.GetFields();
                Assert.NotNull(fields);
                Assert.Equal(field, fields.Single());

                var removedField = index.RemoveField(field);
                Assert.True(removedField);

                var noFields = index.GetFields();
                Assert.NotNull(fields);
                Assert.Empty(noFields);
            }
            finally
            {
                index.Destroy();
                index.Dispose();
            }
        }

        [Fact]
        public void Can_persist_vectors()
        {
            const string key = "Key";

            var index = new LmdbIndex($"{Guid.NewGuid()}");

            try
            {
                Vector vector = VectorFrom(4, 5, 6);
                Assert.Equal(Math.Sqrt(77), vector.Magnitude);

                var addedVector = index.AddFieldVector(key, vector, CancellationToken.None);
                Assert.True(addedVector);

                var getByKey = index.GetFieldVectorByKey(key);
                Assert.NotNull(getByKey);
                Assert.Equal(Math.Sqrt(77), getByKey?.Magnitude);

                var getKeys = index.GetFieldVectorKeys().ToList();
                Assert.Single(getKeys);
                Assert.Equal(getKeys[0], key);
            }
            finally
            {
                index.Destroy();
                index.Dispose();
            }
        }



        private static Vector VectorFrom(params double[] elements)
            => new Vector(elements.Select((el, i) => (i, el)).ToArray());
    }
}
