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
            TokenSet tokenSet = TokenSet.FromArray(new[] { "ac", "dc" });
            TokenSet acnode = tokenSet.Edges['a'].Edges['c'];
            TokenSet dcNode = tokenSet.Edges['d'].Edges['c'];

            Assert.Equal(acnode, dcNode);
        }

        [Fact]
        public void ToEnumerationIncludesAllWords()
        {
            var words = new[] { "bat", "cat" };
            var tokenSet = TokenSet.FromArray(words);

            Assert.True(new HashSet<string>(words).SetEquals(tokenSet.ToEnumeration()));
        }
    }
}
