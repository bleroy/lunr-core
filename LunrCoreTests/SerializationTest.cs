using Lunr;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LunrCoreTests
{
    public class SerializationTest
    {
        private readonly Document[] _documents = new[]
        {
            new Document
            {
                { "id", "a" },
                { "title", "Mr. Green kills Colonel Mustard" },
                { "body", "Mr. Green killed Colonel Mustard in the study with the candlestick. Mr. Green is not a very nice fellow." },
                { "wordCount", 19 }
            },
            new Document
            {
                { "id", "b" },
                { "title", "Plumb waters plant" },
                { "body", "Professor Plumb has a green plant in his study" },
                { "wordCount", 9 }
            },
            new Document
            {
                { "id", "c" },
                { "title", "Scarlett helps Professor" },
                { "body", "Miss Scarlett watered Professor Plumbs green plant while he was away from his office last week." },
                { "wordCount", 16 }
            },
            new Document
            {
                { "id", "d" },
                { "title", "All about JavaScript" },
                { "body", "JavaScript objects have a special __proto__ property" },
                { "wordCount", 7 }
            }
        };

        private static readonly Document[] airports = new[]
        {
            new Document
            {
                { "id", "a" },
                { "ICAO", "LHR" },
                { "IATA", "EGLL" },
                { "Name", "London Heathrow" }
            },
            new Document
            {
                { "id", "b" },
                { "ICAO", "BQH" },
                { "IATA", "EGKB" },
                { "Name", "London Biggin Hill" }
            },
            new Document
            {
                { "id", "c" },
                { "ICAO", "GON" },
                { "IATA", "KGON" },
                { "Name", "Groton-New London" }
            },
            new Document
            {
                { "id", "d" },
                { "ICAO", "(none)" },
                { "IATA", "92TE" },
                { "Name", "Chaney San Francisco Ranch" }
            },
            new Document
            {
                { "id", "e" },
                { "ICAO", "SPSF" },
                { "IATA", "(none)" },
                { "Name", "San Francisco (Peru)" }
            }
        };


        // Serialized index acquired from a debug run of the same test in the lunr.js library.
        private readonly string _lunrJsSerializedIndex = "{\"version\":\"2.3.9\",\"fields\":[\"title\",\"body\"],\"fieldVectors\":[[\"title/a\",[0,0.545,1,0.083,2,0.545,3,0.545,4,0.545]],[\"body/a\",[0,0.845,1,0.128,2,0.584,3,0.584,4,0.584,5,0.584,6,1.015,7,1.015,8,1.015,9,1.015]],[\"title/b\",[10,0.357,11,0.693,12,0.357]],[\"body/b\",[1,0.126,5,0.826,10,0.425,12,0.425,13,0.425]],[\"title/c\",[13,0.357,14,0.693,15,1.204]],[\"body/c\",[1,0.093,10,0.314,11,0.61,12,0.314,13,0.314,14,0.61,16,1.059,17,1.059,18,1.059,19,1.059,20,1.059]],[\"title/d\",[21,0.953]],[\"body/d\",[21,0.826,22,1.435,23,1.435,24,1.435,25,1.435]]],\"invertedIndex\":[[\"__proto__\",{\"_index\":24,\"title\":{},\"body\":{\"d\":{}}}],[\"away\",{\"_index\":17,\"title\":{},\"body\":{\"c\":{}}}],[\"candlestick\",{\"_index\":6,\"title\":{},\"body\":{\"a\":{}}}],[\"colonel\",{\"_index\":3,\"title\":{\"a\":{}},\"body\":{\"a\":{}}}],[\"fellow\",{\"_index\":9,\"title\":{},\"body\":{\"a\":{}}}],[\"green\",{\"_index\":1,\"title\":{\"a\":{}},\"body\":{\"a\":{},\"b\":{},\"c\":{}}}],[\"help\",{\"_index\":15,\"title\":{\"c\":{}},\"body\":{}}],[\"javascript\",{\"_index\":21,\"title\":{\"d\":{}},\"body\":{\"d\":{}}}],[\"kill\",{\"_index\":2,\"title\":{\"a\":{}},\"body\":{\"a\":{}}}],[\"last\",{\"_index\":19,\"title\":{},\"body\":{\"c\":{}}}],[\"miss\",{\"_index\":16,\"title\":{},\"body\":{\"c\":{}}}],[\"mr\",{\"_index\":0,\"title\":{\"a\":{}},\"body\":{\"a\":{}}}],[\"mustard\",{\"_index\":4,\"title\":{\"a\":{}},\"body\":{\"a\":{}}}],[\"nice\",{\"_index\":8,\"title\":{},\"body\":{\"a\":{}}}],[\"object\",{\"_index\":22,\"title\":{},\"body\":{\"d\":{}}}],[\"offic\",{\"_index\":18,\"title\":{},\"body\":{\"c\":{}}}],[\"plant\",{\"_index\":12,\"title\":{\"b\":{}},\"body\":{\"b\":{},\"c\":{}}}],[\"plumb\",{\"_index\":10,\"title\":{\"b\":{}},\"body\":{\"b\":{},\"c\":{}}}],[\"professor\",{\"_index\":13,\"title\":{\"c\":{}},\"body\":{\"b\":{},\"c\":{}}}],[\"properti\",{\"_index\":25,\"title\":{},\"body\":{\"d\":{}}}],[\"scarlett\",{\"_index\":14,\"title\":{\"c\":{}},\"body\":{\"c\":{}}}],[\"special\",{\"_index\":23,\"title\":{},\"body\":{\"d\":{}}}],[\"studi\",{\"_index\":5,\"title\":{},\"body\":{\"a\":{},\"b\":{}}}],[\"veri\",{\"_index\":7,\"title\":{},\"body\":{\"a\":{}}}],[\"water\",{\"_index\":11,\"title\":{\"b\":{}},\"body\":{\"c\":{}}}],[\"week\",{\"_index\":20,\"title\":{},\"body\":{\"c\":{}}}]],\"pipeline\":[\"stemmer\"]}";
        private readonly string _serializeIndexWithNumbersAndUnderscores = "{\"version\":\"2.3.9\",\"fields\":[\"title\"],\"fieldVectors\":[[\"title/underscore\",[0,0.524]],[\"title/zero\",[1,0.524]],[\"title/all the things\",[0,0.39,1,0.39]]],\"invertedIndex\":[[\"0\",{\"_index\":1,\"title\":{\"zero\":{},\"all the things\":{}}}],[\"_\",{\"_index\":0,\"title\":{\"underscore\":{},\"all the things\":{}}}]],\"pipeline\":[\"stemmer\"]}";

        [Fact]
        public async Task CanSerializeIndex()
        {
            Index idx = await Index.Build(async builder =>
            {
                builder.ReferenceField = "id";

                builder
                    .AddField("title")
                    .AddField("body");

                foreach (Document doc in _documents)
                {
                    await builder.Add(doc);
                }
            });

            string serializedIndex = idx.ToJson();

            // We want strict compatibility of index serialization with lunr.js
            Assert.Equal(_lunrJsSerializedIndex, serializedIndex);
        }

        [Fact]
        public void CanDeserializeIndex()
        {
            var deserializedIndex = Index.LoadFromJson(_lunrJsSerializedIndex);

            Assert.Equal(_lunrJsSerializedIndex, deserializedIndex.ToJson());
            Assert.Equal(16, deserializedIndex.TokenSet.Edges.Count);
        }

        [Fact]
        public void CanDeserializeIndexWithDifferentPatchLevel()
        {
            var _lunrJsSerializedIndex2 = _lunrJsSerializedIndex.Replace("\"version\":\"2.3.9\"", "\"version\":\"2.3.10\"");
            var deserializedIndex = Index.LoadFromJson(_lunrJsSerializedIndex2);

            Assert.Equal(16, deserializedIndex.TokenSet.Edges.Count);
        }

        [Fact]
        public async Task CanSerializeIndexInOrderWithUnderscoresAndDigits()
        {
            Index idx = await Index.Build(async builder =>
            {
                builder.ReferenceField = "id";

                builder.AddField("title");

                await builder.Add(new Document
                {
                    { "id", "underscore" },
                    { "title", "_" }
                });

                await builder.Add(new Document
                {
                    { "id", "zero" },
                    { "title", "0" }
                });

                await builder.Add(new Document
                {
                    { "id", "all the things" },
                    { "title", "0 _" }
                });
            });

            string serializedIndex = idx.ToJson();

            // We want strict compatibility of index serialization with lunr.js
            Assert.Equal(_serializeIndexWithNumbersAndUnderscores, serializedIndex);
        }

        [Fact]
        public async Task CanSerializeAndDeserializeIndexWithMetadata()
        {
            var registry = new PipelineFunctionRegistry(
                ("meta", Pipeline.BuildFunction(token =>
                {
                    token.Metadata.Add("null", null);
                    token.Metadata.Add("bool-true", true);
                    token.Metadata.Add("bool-false", false);
                    token.Metadata.Add("number-int", 42);
                    token.Metadata.Add("number-float", 2.4);
                    token.Metadata.Add("string", "foo");
                    token.Metadata.Add("array", new object?[] { 1, false, new[] { 1, 2, 3 }, null, "bar" });
                    token.Metadata.Add("object", new Dictionary<string, object> { { "foo", 3 }, { "bar", "baz" } });
                })));
            Index idx = await Index.Build(async builder =>
            {
                builder
                    .AllowMetadata(
                        "position",
                        "null",
                        "bool-true",
                        "bool-false",
                        "number-int",
                        "number-float", 
                        "string",
                        "array",
                        "object")
                    .AddField("body");

                await builder.Add(new Document
                {
                    {"id", "me"},
                    {"body", "I am developer."}
                });
            },
            registry: registry,
            indexingPipeline: new[] { "meta" });

            string serializedIndex = idx.ToJson();
            var deserializedIndex = Index.LoadFromJson(serializedIndex);

            Assert.Equal(new Slice(5, 10), (Slice?)deserializedIndex.InvertedIndex["developer."]["body"]["me"]["position"].Single());
            Assert.Null(deserializedIndex.InvertedIndex["developer."]["body"]["me"]["null"].Single());
            Assert.Equal(true, deserializedIndex.InvertedIndex["developer."]["body"]["me"]["bool-true"].Single());
            Assert.Equal(false, deserializedIndex.InvertedIndex["developer."]["body"]["me"]["bool-false"].Single());
            Assert.Equal(42.0, deserializedIndex.InvertedIndex["developer."]["body"]["me"]["number-int"].Single());
            Assert.Equal(2.4, deserializedIndex.InvertedIndex["developer."]["body"]["me"]["number-float"].Single());
            Assert.Equal("foo", deserializedIndex.InvertedIndex["developer."]["body"]["me"]["string"].Single());
            var deserializedArray = deserializedIndex.InvertedIndex["developer."]["body"]["me"]["array"].Single() as List<object?>;
            Assert.NotNull(deserializedArray);
            Assert.Equal(1.0, deserializedArray![0]);
            Assert.Equal(false, deserializedArray![1]);
            Assert.Equal(new[] { 1.0, 2.0, 3.0 }, deserializedArray![2]);
            Assert.Null(deserializedArray![3]);
            Assert.Equal("bar", deserializedArray![4]);
            Assert.Equal(new Slice(5, 10), deserializedIndex.InvertedIndex["developer."]["body"]["me"]["position"].Single());
            var deserializedObject = deserializedIndex.InvertedIndex["developer."]["body"]["me"]["object"].Single() as Dictionary<string, object?>;
            Assert.NotNull(deserializedObject);
            Assert.Equal(3.0, deserializedObject!["foo"]);
            Assert.Equal("baz", deserializedObject!["bar"]);
        }

        [Fact]
        public async Task DeserializedIndexIncludesTokenSet()
        {
            Index builtIndex = await Index.Build(async builder =>
            {
                builder.AddField("IATA", boost: 5);
                builder.AddField("ICAO", boost: 10);
                builder.AddField("Name");

                foreach (Document airport in airports)
                {
                    await builder.Add(airport);
                }

            });

            IList<Result> builtResult = await builtIndex.Search("egll").ToList();
            int builtCount = builtResult.Count;

            string JSON = builtIndex.ToJson();
            var jsonIndex = Index.LoadFromJson(JSON);

            IList<Result> jsonResult = await jsonIndex.Search("egll").ToList();
            int jsonCount = jsonResult.Count;

            Assert.Equal(builtCount, jsonCount);
            Assert.Equal(builtIndex.TokenSet.Edges.Count, jsonIndex.TokenSet.Edges.Count);
        }
    }
}
