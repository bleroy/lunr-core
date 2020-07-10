using Lunr;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LunrCoreTests
{
    public class TokenSetTests
    {
        [Fact]
        public void ToStringIncludesNodeFinality()
        {
            var nonFinal = new TokenSet();
            var final = new TokenSet { IsFinal = true };
            var otherFinal = new TokenSet { IsFinal = true };

            Assert.NotEqual(nonFinal.ToString(), final.ToString());
            Assert.Equal(otherFinal.ToString(), final.ToString());
        }

        [Fact]
        public void ToStringIncludesAllEdges()
        {
            var zeroEdges = new TokenSet();
            var oneEdge = new TokenSet();
            var twoEdges = new TokenSet();

            oneEdge.Edges['a'] = new TokenSet();
            twoEdges.Edges['a'] = new TokenSet();
            twoEdges.Edges['b'] = new TokenSet();

            Assert.NotEqual(zeroEdges.ToString(), oneEdge.ToString());
            Assert.NotEqual(twoEdges.ToString(), oneEdge.ToString());
            Assert.NotEqual(twoEdges.ToString(), zeroEdges.ToString());
        }

        [Fact]
        public void ToStringIncludesEdgeId()
        {
            var childA = new TokenSet();
            var childB = new TokenSet();
            var parentA = new TokenSet();
            var parentB = new TokenSet();
            var parentC = new TokenSet();

            parentA.Edges['a'] = childA;
            parentB.Edges['a'] = childB;
            parentC.Edges['a'] = childB;

            Assert.Equal(parentB.ToString(), parentC.ToString());
            Assert.NotEqual(parentA.ToString(), parentC.ToString());
            Assert.NotEqual(parentA.ToString(), parentB.ToString());
        }

        [Fact]
        public void FromStringWithoutWildCard()
        {
            var idProvider = new TokenSetIdProvider();
            var x = TokenSet.FromString("a", idProvider);

            Assert.Equal("0a2", x.ToString());
            Assert.True(x.Edges['a'].IsFinal);
        }

        [Fact]
        public void FromStringWithTrailingWildcard()
        {
            var x = TokenSet.FromString("a*");
            TokenSet wild = x.Edges['a'].Edges['*'];

            // A state reached by a wildcard has an edge with a wildcard to itself.
            // The resulting automota is non-determenistic.
            Assert.Equal(wild, wild.Edges['*']);
            Assert.True(wild.IsFinal);
        }

        [Fact]
        public void FromArrayWithUnsortedArrayThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                TokenSet.FromArray(new[] { "z", "a" });
            });
        }

        [Fact]
        public void FromArrayWithSortedArray()
        {
            var tokenSet = TokenSet.FromArray(new[] { "a", "z" });
            Assert.Equal(
                new[] { "a", "z" },
                tokenSet.ToEnumeration().OrderBy(t => t));
        }

        [Fact]
        public void FromArrayIsMinimal()
        {
            var tokenSet = TokenSet.FromArray(new[] { "ac", "dc" });
            TokenSet acnode = tokenSet.Edges['a'].Edges['c'];
            TokenSet dcNode = tokenSet.Edges['d'].Edges['c'];

            Assert.Equal(acnode, dcNode);
        }

        [Fact]
        public void ToEnumerationIncludesAllWords()
        {
            string[] words = new[] { "bat", "cat" };
            var tokenSet = TokenSet.FromArray(words);

            Assert.True(new HashSet<string>(words).SetEquals(tokenSet.ToEnumeration()));
        }

        [Fact]
        public void ToEnumerateIncludesSingleWords()
        {
            string word = "bat";
            var tokenSet = TokenSet.FromString(word);

            Assert.Equal(new[] { word }, tokenSet.ToEnumeration());
        }

        [Fact]
        public void IntersectNonOverlappingIsEmpty()
        {
            var x = TokenSet.FromString("cat");
            var y = TokenSet.FromString("bar");
            TokenSet z = x.Intersect(y);

            Assert.Empty(z.ToEnumeration());
        }

        [Fact]
        public void IntersectForSimpleIntersection()
        {
            var x = TokenSet.FromString("cat");
            var y = TokenSet.FromString("cat");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "cat" }, z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithTrailingWildcardIntersection()
        {
            var x = TokenSet.FromString("cat");
            var y = TokenSet.FromString("c*");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "cat" }, z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithTrailingWildcardNoIntersection()
        {
            var x = TokenSet.FromString("cat");
            var y = TokenSet.FromString("b*");
            TokenSet z = x.Intersect(y);

            Assert.Empty(z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithLeadingWildcardIntersection()
        {
            var x = TokenSet.FromString("cat");
            var y = TokenSet.FromString("*t");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "cat" }, z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithLeadingWildcardBackTrackingIntersection()
        {
            var x = TokenSet.FromString("aaacbab");
            var y = TokenSet.FromString("*ab");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "aaacbab" }, z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithLeadingWildcardNoIntersection()
        {
            var x = TokenSet.FromString("cat");
            var y = TokenSet.FromString("*r");
            TokenSet z = x.Intersect(y);

            Assert.Empty(z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithLeadingWildcardBackTrackingNoIntersection()
        {
            var x = TokenSet.FromString("aaabdcbc");
            var y = TokenSet.FromString("*abc");
            TokenSet z = x.Intersect(y);

            Assert.Empty(z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithContainedWildcardIntersection()
        {
            var x = TokenSet.FromString("foo");
            var y = TokenSet.FromString("f*o");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "foo" }, z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithContainedWildcardBacktrackingIntersection()
        {
            var x = TokenSet.FromString("ababc");
            var y = TokenSet.FromString("a*bc");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "ababc" }, z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithContainedWildcardNoIntersection()
        {
            var x = TokenSet.FromString("foo");
            var y = TokenSet.FromString("b*r");
            TokenSet z = x.Intersect(y);

            Assert.Empty(z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithContainedWildcardBacktrackingNoIntersection()
        {
            var x = TokenSet.FromString("ababc");
            var y = TokenSet.FromString("a*ac");
            TokenSet z = x.Intersect(y);

            Assert.Empty(z.ToEnumeration());
        }

        [Fact]
        public void IntersectWiildcardMatchesZeroOrMoreCharacters()
        {
            var x = TokenSet.FromString("foo");
            var y = TokenSet.FromString("foo*");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "foo" }, z.ToEnumeration());
        }

        // This test is intended to prevent 'bugs' that have lead to these
        // kind of intersections taking a _very_ long time. The assertion
        // is not of interest, just that the test does not timeout.
        [Fact]
        public void CatastrophicBacktrackingWithLeadingCharacters()
        {
            var x = TokenSet.FromString("fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff");
            var y = TokenSet.FromString("*ff");
            TokenSet z = x.Intersect(y);

            Assert.Single(z.ToEnumeration());
        }

        [Fact]
        public void IntersectLeadingAndTrailingBacktrackingIntersection()
        {
            var x = TokenSet.FromString("acbaabab");
            var y = TokenSet.FromString("*ab*");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "acbaabab" }, z.ToEnumeration());
        }

        [Fact]
        public void IntersectMultipleContainedWildcardBackTracking()
        {
            var x = TokenSet.FromString("acbaabab");
            var y = TokenSet.FromString("a*ba*b");
            TokenSet z = x.Intersect(y);

            Assert.Equal(new[] { "acbaabab" }, z.ToEnumeration());
        }

        [Fact]
        public void IntersectWithFuzzyStringSubstitution()
        {
            var x1 = TokenSet.FromString("bar");
            var x2 = TokenSet.FromString("cur");
            var x3 = TokenSet.FromString("cat");
            var x4 = TokenSet.FromString("car");
            var x5 = TokenSet.FromString("foo");
            var y = TokenSet.FromFuzzyString("car", 1);

            Assert.Equal(new[] { "bar" }, x1.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "cur" }, x2.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "cat" }, x3.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "car" }, x4.Intersect(y).ToEnumeration());
            Assert.Empty(x5.Intersect(y).ToEnumeration());
        }

        [Fact]
        public void IntersectWithFuzzyStringDeletion()
        {
            var x1 = TokenSet.FromString("ar");
            var x2 = TokenSet.FromString("br");
            var x3 = TokenSet.FromString("ba");
            var x4 = TokenSet.FromString("bar");
            var x5 = TokenSet.FromString("foo");
            var y = TokenSet.FromFuzzyString("bar", 1);

            Assert.Equal(new[] { "ar" }, x1.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "br" }, x2.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "ba" }, x3.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "bar" }, x4.Intersect(y).ToEnumeration());
            Assert.Empty(x5.Intersect(y).ToEnumeration());
        }

        [Fact]
        public void IntersectWithFuzzyStringInsertion()
        {
            var x1 = TokenSet.FromString("bbar");
            var x2 = TokenSet.FromString("baar");
            var x3 = TokenSet.FromString("barr");
            var x4 = TokenSet.FromString("bar");
            var x5 = TokenSet.FromString("ba");
            var x6 = TokenSet.FromString("foo");
            var x7 = TokenSet.FromString("bara");
            var y = TokenSet.FromFuzzyString("bar", 1);

            Assert.Equal(new[] { "bbar" }, x1.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "baar" }, x2.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "barr" }, x3.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "bar" }, x4.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "ba" }, x5.Intersect(y).ToEnumeration());
            Assert.Empty(x6.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "bara" }, x7.Intersect(y).ToEnumeration());
        }

        [Fact]
        public void IntersectWithFuzzyStringTranspose()
        {
            var x1 = TokenSet.FromString("abr");
            var x2 = TokenSet.FromString("bra");
            var x3 = TokenSet.FromString("foo");
            var y = TokenSet.FromFuzzyString("bar", 1);

            Assert.Equal(new[] { "abr" }, x1.Intersect(y).ToEnumeration());
            Assert.Equal(new[] { "bra" }, x2.Intersect(y).ToEnumeration());
            Assert.Empty(x3.Intersect(y).ToEnumeration());
        }

        [Fact]
        public void IntersectWithFuzzierStringInsertion()
        {
            var x = TokenSet.FromString("abcxx");
            var y = TokenSet.FromFuzzyString("abc", 2);

            Assert.Equal(new[] { "abcxx" }, x.Intersect(y).ToEnumeration());
        }

        [Fact]
        public void IntersectWithFuzzierStringSubstitution()
        {
            var x = TokenSet.FromString("axx");
            var y = TokenSet.FromFuzzyString("abc", 2);

            Assert.Equal(new[] { "axx" }, x.Intersect(y).ToEnumeration());
        }

        [Fact]
        public void IntersectWithFuzzierStringDeletion()
        {
            var x = TokenSet.FromString("a");
            var y = TokenSet.FromFuzzyString("abc", 2);

            Assert.Equal(new[] { "a" }, x.Intersect(y).ToEnumeration());
        }

        [Fact]
        public void IntersectWithFuzzierStringTranspose()
        {
            var x = TokenSet.FromString("bca");
            var y = TokenSet.FromFuzzyString("abc", 2);

            Assert.Equal(new[] { "bca" }, x.Intersect(y).ToEnumeration());
        }
    }
}
