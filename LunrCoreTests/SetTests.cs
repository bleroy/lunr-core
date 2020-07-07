using Lunr;
using Xunit;

namespace LunrCoreTests
{
    public class SetTests
    {
        [Fact]
        public void CompleteSetContainsEverything()
        {
            Assert.True(Set<string>.Complete.Contains("foo"));
        }

        [Fact]
        public void EmptySetContainsNothing()
        {
            Assert.False(Set<string>.Empty.Contains("foo"));
        }

        [Fact]
        public void SetContainsItsElements()
        {
            Assert.True(new Set<string>("foo").Contains("foo"));
        }

        [Fact]
        public void SetDoesNotContainsNonElements()
        {
            Assert.False(new Set<string>("foo").Contains("bar"));
        }

        [Fact]
        public void CompleteSetUnionPopulatedSetContainsEverything()
        {
            ISet<string> union = Set<string>.Complete.Union(new Set<string>("foo"));
            Assert.True(union.Contains("foo"));
            Assert.True(union.Contains("bar"));
        }

        [Fact]
        public void EmptySetUnionPopulatedSetIsThePopulatedSet()
        {
            ISet<string> union = Set<string>.Empty.Union(new Set<string>("foo"));
            Assert.True(union.Contains("foo"));
            Assert.False(union.Contains("bar"));
        }

        [Fact]
        public void UnionContainsElementsFromBothSets()
        {
            ISet<string> union = new Set<string>("bar")
                .Union(new Set<string>("foo"));

            Assert.True(union.Contains("foo"));
            Assert.True(union.Contains("bar"));
            Assert.False(union.Contains("baz"));
        }

        [Fact]
        public void UnionWithEmptySetContainsAllElements()
        {
            ISet<string> union = new Set<string>("bar")
                .Union(Set<string>.Empty);

            Assert.True(union.Contains("bar"));
            Assert.False(union.Contains("baz"));
        }

        [Fact]
        public void UnionWithCompleteSetContainsEverything()
        {
            ISet<string> union = new Set<string>("bar")
                .Union(Set<string>.Complete);

            Assert.True(union.Contains("foo"));
            Assert.True(union.Contains("bar"));
            Assert.True(union.Contains("baz"));
        }

        [Fact]
        public void CompleteSetInterPopulatedSetIsThatSet()
        {
            ISet<string> inter = Set<string>.Complete
                .Intersect(new Set<string>("foo"));

            Assert.True(inter.Contains("foo"));
            Assert.False(inter.Contains("bar"));
        }

        [Fact]
        public void EmptySetInterPopulatedSetIsEmpty()
        {
            ISet<string> inter = Set<string>.Empty
                .Intersect(new Set<string>("foo"));

            Assert.False(inter.Contains("foo"));
        }

        [Fact]
        public void IntersectionOfTwoNonOverlappingSetsIsEmpty()
        {
            ISet<string> inter = new Set<string>("bar")
                .Intersect(new Set<string>("foo"));

            Assert.False(inter.Contains("foo"));
            Assert.False(inter.Contains("bar"));
        }

        [Fact]
        public void IntersectionOfTwoOverlappingSetsContainsIntersectionElements()
        {
            ISet<string> inter = new Set<string>("foo", "bar")
                .Intersect(new Set<string>("foo"));

            Assert.True(inter.Contains("foo"));
            Assert.False(inter.Contains("bar"));
        }

        [Fact]
        public void IntersectionOfPopulatedSetWithEmptySetIsEmpty()
        {
            ISet<string> inter = new Set<string>("foo")
                .Intersect(Set<string>.Empty);

            Assert.False(inter.Contains("foo"));
        }

        [Fact]
        public void IntersectionOfCompleteSetWithPopulatedSetContainsSetElements()
        {
            ISet<string> inter = new Set<string>("foo")
                .Intersect(Set<string>.Complete);

            Assert.True(inter.Contains("foo"));
            Assert.False(inter.Contains("bar"));
        }
    }
}
