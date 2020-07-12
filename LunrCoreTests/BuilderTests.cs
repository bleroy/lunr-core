using Lunr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Index = Lunr.Index;

namespace LunrCoreTests
{
    public class BuilderTests
    {
        // The next few tests really only makes sense in the Javascript version.
        // Including them anyways.
        [Fact]
        public async Task FieldContainsTermsThatClashWithObjectPrototype()
        {
            var builder = new Builder();
            builder.AddField("title");

            await builder.Add(new Document
            {
                { "id", "id" },
                { "title", "constructor" }
            }, new CancellationToken());

            Assert.Empty(builder.InvertedIndex["constructor"]["title"]["id"]);
            Assert.Equal(1,
                builder.FieldTermFrequencies
                    [FieldReference.FromString("title/id")]
                    [new Token("constructor")]);
        }

        [Fact]
        public async Task FieldNameClashesWithObjectPrototype()
        {
            var builder = new Builder();
            builder.AddField("constructor");
            
            await builder.Add(new Document
            {
                { "id", "id" },
                { "constructor", "constructor" }
            }, new CancellationToken());

            Assert.Empty(builder.InvertedIndex["constructor"]["constructor"]["id"]);
        }

        [Fact]
        public async Task DocumentRefClashesWithObjectPrototype()
        {
            var builder = new Builder();
            builder.AddField("title");

            await builder.Add(new Document
            {
                { "id", "constructor" },
                { "title", "word" }
            }, new CancellationToken());

            Assert.Empty(builder.InvertedIndex["word"]["title"]["constructor"]);
        }

        [Fact]
        public async Task TokenMetadataClashesWithObjectPrototype()
        {
            Func<Token, Token> fn = t =>
            {
                t.Metadata["constructor"] = "foo";
                return t;
            };
            Pipeline.Function pipelineFunction = fn.ToPipelineFunction();

            var builder = new Builder();
            builder.IndexingPipeline.RegisterFunction(pipelineFunction, "test");
            builder.IndexingPipeline.Add(pipelineFunction);
            builder.MetadataWhiteList.Add("constructor");

            builder.AddField("title");

            await builder.Add(new Document
            {
                { "id", "id" },
                { "title", "word" }
            }, new CancellationToken());

            Assert.Equal(
                new[] { "foo" },
                builder.InvertedIndex["word"]["title"]["id"]["constructor"]);
        }

        [Fact]
        public async Task ExtractingNestedPropertiesFromADocument()
        {
            var builder = new Builder(
                new Field<string>("name", extractor: ExtractName));

            await builder.Add(new Document
            {
                { "id", "id" },
                {
                    "person",
                    new Dictionary<string, string>
                    {
                        { "name", "bob" }
                    }
                }
            }, new CancellationToken());

            Assert.Empty(builder.InvertedIndex["bob"]["name"]["id"]);
        }

        private static async Task<string> ExtractName(Document doc)
            => await Task.FromResult(((IDictionary<string, string>)doc["person"])["name"]);

        [Fact]
        public void DefiningFieldsToIndex()
        {
            var builder = new Builder();
            builder.AddField("foo");
            Assert.Equal(new[] { "foo" }, builder.Fields.Select(f => f.Name));
        }

        [Fact]
        public void FieldWithIllegalCharactersThrows()
        {
            var builder = new Builder();
            Assert.Throws<InvalidOperationException>(() =>
            {
                builder.AddField("foo/bar");
            });
        }

        [Fact]
        public void DefaultReferenceFieldIsId()
        {
            var builder = new Builder();
            Assert.Equal("id", builder.ReferenceField);
        }

        [Fact]
        public void DefiningAReferenceField()
        {
            var builder = new Builder
            {
                ReferenceField = "foo"
            };
            Assert.Equal("foo", builder.ReferenceField);
        }

        [Fact]
        public void FieldLengthNormalizationFactorIsCoercedToRange()
        {
            var builder = new Builder();
            Assert.Equal(0.75, builder.FieldLengthNormalizationFactor);

            builder.FieldLengthNormalizationFactor = -1;
            Assert.Equal(0, builder.FieldLengthNormalizationFactor);

            builder.FieldLengthNormalizationFactor = 1.5;
            Assert.Equal(1, builder.FieldLengthNormalizationFactor);

            builder.FieldLengthNormalizationFactor = 0.5;
            Assert.Equal(0.5, builder.FieldLengthNormalizationFactor);
        }

        [Fact]
        public void TermFrequencySaturationFactor()
        {
            var builder = new Builder();
            Assert.Equal(1.2, builder.TermFrequencySaturationFactor);

            builder.TermFrequencySaturationFactor = 1.6;
            Assert.Equal(1.6, builder.TermFrequencySaturationFactor);
        }

        [Fact]
        public async Task BuilderBuildsInvertedIndex()
        {
            var builder = new Builder();
            builder.AddField("title");
            var cancellationToken = new CancellationToken();
            await builder.Add(new Document
            {
                { "id", "id" },
                { "title", "test" },
                { "body", "missing" }
            }, cancellationToken);
            Index index = builder.Build();

            Assert.Empty(builder.InvertedIndex["test"]["title"]["id"]);

            Assert.IsType<Vector>(builder.FieldVectors["title/id"]);

            Assert.False(builder.InvertedIndex.ContainsKey("missing"));

            var needle = TokenSet.FromString("test");
            Assert.Contains("test", builder.TokenSet.Intersect(needle).ToEnumeration());

            Assert.Equal(1, builder.DocumentCount);

            Assert.Equal(1, builder.AverageFieldLength["title"]);

            Assert.NotNull(index);
        }
    }
}
