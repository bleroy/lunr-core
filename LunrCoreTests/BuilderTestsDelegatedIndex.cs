using Lunr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Index = Lunr.DelegatedIndex;

namespace LunrCoreTests
{
    public class BuilderTestsDelegatedIndex
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
            });

            var index = builder.Build().AsDelegated();

            Assert.Empty(index.GetInvertedIndexEntryByKey("constructor")["title"]["id"]);
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
            });

            var index = builder.Build().AsDelegated();

            Assert.Empty(index.GetInvertedIndexEntryByKey("constructor")["constructor"]["id"]);
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
            });

            var index = builder.Build().AsDelegated();

            Assert.Empty(index.GetInvertedIndexEntryByKey("word")["title"]["constructor"]);
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
            builder.MetadataAllowList.Add("constructor");

            builder.AddField("title");

            await builder.Add(new Document
            {
                { "id", "id" },
                { "title", "word" }
            });

            var index = builder.Build().AsDelegated();

            Assert.Equal(
                new[] { "foo" },
                index.GetInvertedIndexEntryByKey("word")["title"]["id"]["constructor"]);
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
            });
            
            var index = builder.Build().AsDelegated();

            Assert.Empty(index.GetInvertedIndexEntryByKey("bob")["name"]["id"]);
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
            await builder.Add(new Document
            {
                { "id", "id" },
                { "title", "test" },
                { "body", "missing" }
            });

            Index index = builder.Build().AsDelegated();

            Assert.Empty(builder.InvertedIndex["test"]["title"]["id"]);

            Assert.IsType<Vector>(builder.FieldVectors["title/id"]);

            Assert.False(builder.InvertedIndex.ContainsKey("missing"));

            var needle = TokenSet.FromString("test");
            Assert.Contains("test", builder.TokenSet.Intersect(needle).ToEnumeration());

            Assert.Equal(1, builder.DocumentCount);

            Assert.Equal(1, builder.AverageFieldLength["title"]);

            Assert.NotNull(index);
        }

        [Fact]
        public async Task BuilderCanIncludeTokenPositions()
        { 
            Index index = (await Lunr.Index.Build(async builder =>
            {
                builder.MetadataAllowList.Add("position");
                builder.AddField("href", 3);
                builder.AddField("title", 2);
                builder.AddField("body", 1);

                await builder.Add(new Document
                {
                    {"id", "me"},
                    {"href", "http://bertrandleroy.net"},
                    {"title", "Bertrand"},
                    {"body", "I am developer."}
                });
            })).AsDelegated();

            Result developer = (await index.Search("developer").ToList()).Single();
            Assert.Equal(new Slice(5, 10), (Slice?)developer.MatchData.Posting["develop"]["body"]["position"].Single());
        }
        
        [Fact]
        public async Task BuilderWithCustomSeparator()
        {
            var builder = new Builder();
            builder.AddField("title");

            var regex = new Regex(@"[^\w]");
            builder.Separator = c => regex.IsMatch(c.ToString());
            
            await builder.Add(new Document
            {
                { "id", "id" },
                { "title", "constructor(this,is,special)" }
            });

            var index = builder.Build().AsDelegated();

           
            Assert.Equal(4, builder.InvertedIndex.Count);
            Assert.Empty(index.GetInvertedIndexEntryByKey("constructor")["title"]["id"]);
            Assert.Empty(index.GetInvertedIndexEntryByKey("this")["title"]["id"]);
            Assert.Empty(index.GetInvertedIndexEntryByKey("is")["title"]["id"]);
            Assert.Empty(index.GetInvertedIndexEntryByKey("special")["title"]["id"]);
            Assert.Equal(1, builder.FieldTermFrequencies[FieldReference.FromString("title/id")][new Token("constructor")]);
            Assert.Equal(1, builder.FieldTermFrequencies[FieldReference.FromString("title/id")][new Token("this")]);
            Assert.Equal(1, builder.FieldTermFrequencies[FieldReference.FromString("title/id")][new Token("is")]);
            Assert.Equal(1, builder.FieldTermFrequencies[FieldReference.FromString("title/id")][new Token("special")]);
        }
    }
}
