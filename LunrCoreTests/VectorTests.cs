using Lunr;
using System;
using System.Linq;
using Xunit;

namespace LunrCoreTests
{
    public class VectorTests
    {
        [Fact]
        public void CalculatesMagnitudeOfAVector()
        {
            Vector vector = VectorFrom(4, 5, 6);
            Assert.Equal(Math.Sqrt(77), vector.Magnitude);
        }

        [Fact]
        public void CalculatesDotProductOfTwoVectors()
        {
            Vector v1 = VectorFrom(1, 3, -5),
                v2 = VectorFrom(4, -2, -1);

            Assert.Equal(3, v1.Dot(v2));
        }

        [Fact]
        public void CalculatesTheSimilarityBetweenTwoVectors()
        {
            Vector v1 = VectorFrom(1, 3, -5),
                v2 = VectorFrom(4, -2, -1);

            Assert.InRange(v1.Similarity(v2), 0.49, 0.51);
        }

        [Fact]
        public void EmptyVectorSimilarityIsZero()
        {
            Vector empty = new Vector(),
                v1 = VectorFrom(1);

            Assert.Equal(0, empty.Similarity(v1));
            Assert.Equal(0, v1.Similarity(empty));
        }

        [Fact]
        public void NonOverlappingVectorsAreNotSimilar()
        {
            var v1 = new Vector((1, 1));
            var v2 = new Vector((2, 1));

            Assert.Equal(0, v1.Similarity(v2));
            Assert.Equal(0, v2.Similarity(v1));
        }

        [Fact]
        public void InsertInvalidatesMagnitudeCache()
        {
            Vector vector = VectorFrom(4, 5, 6);

            Assert.Equal(Math.Sqrt(77), vector.Magnitude);

            vector.Insert(3, 7);

            Assert.Equal(Math.Sqrt(126), vector.Magnitude);
        }

        [Fact]
        public void InsertKeepsItemsInTheIndexSpecifiedOrder()
        {
            var vector = new Vector();

            vector.Insert(2, 4);
            vector.Insert(1, 5);
            vector.Insert(0, 6);

            Assert.Equal(new[] { 6.0, 5.0, 4.0 }, vector.ToArray());
        }

        [Fact]
        public void InsertFailsWhenDuplicateEntry()
        {
            Vector vector = VectorFrom(4, 5, 6);
            Assert.Throws<InvalidOperationException>(() => 
            {
                vector.Insert(0, 44);
            });
        }

        [Fact]
        public void UpsertInvalidatesMagnitudeCache()
        {
            Vector vector = VectorFrom(4, 5, 6);

            Assert.Equal(Math.Sqrt(77), vector.Magnitude);

            vector.Upsert(3, 7);

            Assert.Equal(Math.Sqrt(126), vector.Magnitude);
        }

        [Fact]
        public void UpsertKeepsItemsInTheIndexSpecifiedOrder()
        {
            var vector = new Vector();

            vector.Upsert(2, 4);
            vector.Upsert(1, 5);
            vector.Upsert(0, 6);

            Assert.Equal(new[] { 6.0, 5.0, 4.0 }, vector.ToArray());
        }

        [Fact]
        public void UpsertCallsFnForValueOnDuplicate()
        {
            Vector vector = VectorFrom(4, 5, 6);
            vector.Upsert(0, 4, (current, passed) => current + passed);
            Assert.Equal(new[] { 8.0, 5.0, 6.0 }, vector.ToArray());
        }

        [Fact]
        public void PositionForIndex()
        {
            var vector = new Vector(
                (1, 'a'),
                (2, 'b'),
                (4, 'c'),
                (7, 'd'),
                (11, 'e'));

            // At the beginning
            Assert.Equal(0, vector.PositionForIndex(0));
            // At the end
            Assert.Equal(5, vector.PositionForIndex(20));
            // Consecutive
            Assert.Equal(2, vector.PositionForIndex(3));
            // Non-consecutive gap after
            Assert.Equal(3, vector.PositionForIndex(5));
            // Non-consecutive gap before
            Assert.Equal(3, vector.PositionForIndex(6));
            // Non-consecutive gaps before and after
            Assert.Equal(4, vector.PositionForIndex(9));
            // Duplicate at the beginning
            Assert.Equal(0, vector.PositionForIndex(1));
            // Duplicate at the end
            Assert.Equal(4, vector.PositionForIndex(11));
            // Duplicate consecutive
            Assert.Equal(2, vector.PositionForIndex(4));
        }

        private static Vector VectorFrom(params double[] elements)
            => new Vector(elements.Select((el, i) => (i, el)).ToArray());
    }
}
