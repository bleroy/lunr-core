using Lunr;
using System.Collections.Generic;
using Xunit;

namespace LunrCoreTests
{
    public class TokenTests
    {
        [Fact]
        public void TokenConvertsToString()
        {
            var token = new Token("foo");
            Assert.Equal("foo", token);
            Assert.Equal("foo", token.ToString());
            Assert.Equal("foo", token.String);
        }

        [Fact]
        public void CanAttachArbitraryData()
        {
            var token = new Token("foo", ("length", 3));
            Assert.Equal(3, token.Metadata["length"]);
        }

        [Fact]
        public void CanUpdateTheTokenValue()
        {
            var token = new Token("foo");

            token.Update(s => s.ToUpperInvariant());

            Assert.Equal("FOO", token);
        }

        [Fact]
        public void MetadataIsYieldedWhenUpdating()
        {
            var metadata = new TokenMetadata { { "bar", true } };
            var token = new Token("foo", metadata);
            TokenMetadata? yieldedMetadata = null;

            token.Update((s, md) =>
            {
                yieldedMetadata = md;
                return s;
            });

            Assert.Equal(metadata, yieldedMetadata);
        }

        [Fact]
        public void CloneClonesValues()
        {
            var token = new Token("foo", ("bar", true));
            Assert.Equal(token.ToString(), token.Clone().ToString());
        }

        [Fact]
        public void CloneClonesMetadata()
        {
            var token = new Token("foo", ("bar", true));
            Assert.Equal(token.Metadata, token.Clone().Metadata);
        }

        [Fact]
        public void CloneAndModify()
        {
            var token = new Token("foo", ("bar", true));
            Token clone = token.Clone(s => s.ToUpperInvariant());

            Assert.Equal("FOO", clone);
            Assert.Equal("foo", token);
            Assert.Equal(token.Metadata, clone.Metadata);
        }
    }
}
