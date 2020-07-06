using Lunr;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LunrCoreTests
{
    public class TokenizerTests
    {
        [Fact]
        public void SplittingIntoTokens()
        {
            IEnumerable<string> tokens = new Tokenizer()
                .Tokenize("foo bar baz")
                .Select(t => t.String);

            Assert.Equal(new[] { "foo", "bar", "baz" }, tokens);
        }

        [Fact]
        public void DownCasesTokens()
        {
            IEnumerable<string> tokens = new Tokenizer()
                .Tokenize("Foo BAR BAZ")
                .Select(t => t.String);

            Assert.Equal(new[] { "foo", "bar", "baz" }, tokens);
        }

        [Fact]
        public void ArrayOfStrings()
        {
            IEnumerable<string> tokens = new Tokenizer()
                .Tokenize(new[] { "foo", "bar", "baz" })
                .Select(t => t.String);

            Assert.Equal(new[] { "foo", "bar", "baz" }, tokens);
        }

        [Fact]
        public void NullIsConvertedToEmptyString()
        {
            IEnumerable<string> tokens = new Tokenizer()
                .Tokenize(new[] { "foo", null, "baz" })
                .Select(t => t.String);

            Assert.Equal(new[] { "foo", "", "baz" }, tokens);
        }

        [Fact]
        public void MultipleWhitespaceIsStripped()
        {
            IEnumerable<string> tokens = new Tokenizer()
                .Tokenize("   foo    bar   baz  ")
                .Select(t => t.String);

            Assert.Equal(new[] { "foo", "bar", "baz" }, tokens);
        }

        [Fact]
        public void HandlingNullArguments()
        {
            Assert.Empty(new Tokenizer().Tokenize((object)null));
        }

        [Fact]
        public void ConvertingADateToTokens()
        {
            var date = new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            // NOTE: slicing here to prevent asserting on parts
            // of the date that might be affected by the timezone
            // the test is running in.
            IEnumerable<string> tokenizedDateSlice = new Tokenizer()
                .Tokenize(date)
                .Take(4)
                .Select(t => t.ToString());

            Assert.Equal(new[] { "tue", "jan", "01", "2013" }, tokenizedDateSlice);
        }

        [Fact]
        public void ConvertingANumberToTokens()
        {
            Assert.Equal("41", new Tokenizer().Tokenize(41).First().String);
        }

        [Fact]
        public void ConvertingABooleanToTokens()
        {
            Assert.Equal("false", new Tokenizer().Tokenize(false).First().String);
            Assert.Equal("true", new Tokenizer().Tokenize(true).First().String);
        }

        [Fact]
        public void ConvertingAnObjectToTokens()
        {
            Assert.Equal(
                new[] { "custom", "object" },
                new Tokenizer()
                    .Tokenize(new CustomTestObject())
                    .Select(t => t.ToString()));
        }

        private class CustomTestObject
        {
            public override string ToString() => "custom object";
        }

        [Fact]
        public void SplitsStringsWithHyphens()
        {
            Assert.Equal(
                new[] { "foo", "bar" },
                new Tokenizer()
                    .Tokenize("foo-bar")
                    .Select(t => t.ToString()));
        }

        [Fact]
        public void SplitsStringsWithHyphensAndSpaces()
        {
            Assert.Equal(
                new[] { "foo", "bar" },
                new Tokenizer()
                    .Tokenize("foo - bar")
                    .Select(t => t.ToString()));
        }

        [Fact]
        public void TrackingTheTokenIndex()
        {
            IEnumerable<Token> tokens = new Tokenizer().Tokenize("foo bar");
            Assert.Equal(
                new[] { 0, 1 },
                tokens.Select(t => (int)t.Metadata["index"]));
        }

        [Fact]
        public void TrackingTheTokenPosition()
        {
            IEnumerable<Token> tokens = new Tokenizer().Tokenize("foo bar");
            Assert.Equal(
                new[] { (0, 3), (4, 3) },
                tokens.Select(t => ((int, int))t.Metadata["position"]));
        }

        [Fact]
        public void TrackingTheTokenPositionWithAdditionalLeftHandWhiteSpace()
        {
            IEnumerable<Token> tokens = new Tokenizer().Tokenize(" foo bar");
            Assert.Equal(
                new[] { (1, 3), (5, 3) },
                tokens.Select(t => ((int, int))t.Metadata["position"]));
        }

        [Fact]
        public void TrackingTheTokenPositionWithAdditionalRightHandWhiteSpace()
        {
            IEnumerable<Token> tokens = new Tokenizer().Tokenize("foo bar ");
            Assert.Equal(
                new[] { (0, 3), (4, 3) },
                tokens.Select(t => ((int, int))t.Metadata["position"]));
        }

        [Fact]
        public  void ProvidingAdditionalMetadata()
        {
            IEnumerable<Token> tokens = new Tokenizer().Tokenize(
                "foo bar",
                new Dictionary<string, object> { { "hurp", "durp" } });
            Assert.Equal(
                new[] { "durp", "durp" },
                tokens.Select(t => (string)t.Metadata["hurp"]));
        }
    }
}
