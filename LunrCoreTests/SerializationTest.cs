using Lunr;
using System.Text.Json;
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

        // Serialized index acquired from a debug run of the same test in the lunr.js library.
        private readonly string _lunrJsSerializedIndex = "{\"version\":\"2.3.8\",\"fields\":[\"title\",\"body\"],\"fieldVectors\":[[\"title/a\",[0,0.545,1,0.083,2,0.545,3,0.545,4,0.545]],[\"body/a\",[0,0.845,1,0.128,2,0.584,3,0.584,4,0.584,5,0.584,6,1.015,7,1.015,8,1.015,9,1.015]],[\"title/b\",[10,0.357,11,0.693,12,0.357]],[\"body/b\",[1,0.126,5,0.826,10,0.425,12,0.425,13,0.425]],[\"title/c\",[13,0.357,14,0.693,15,1.204]],[\"body/c\",[1,0.093,10,0.314,11,0.61,12,0.314,13,0.314,14,0.61,16,1.059,17,1.059,18,1.059,19,1.059,20,1.059]],[\"title/d\",[21,0.953]],[\"body/d\",[21,0.826,22,1.435,23,1.435,24,1.435,25,1.435]]],\"invertedIndex\":[[\"__proto__\",{\"_index\":24,\"title\":{},\"body\":{\"d\":{}}}],[\"away\",{\"_index\":17,\"title\":{},\"body\":{\"c\":{}}}],[\"candlestick\",{\"_index\":6,\"title\":{},\"body\":{\"a\":{}}}],[\"colonel\",{\"_index\":3,\"title\":{\"a\":{}},\"body\":{\"a\":{}}}],[\"fellow\",{\"_index\":9,\"title\":{},\"body\":{\"a\":{}}}],[\"green\",{\"_index\":1,\"title\":{\"a\":{}},\"body\":{\"a\":{},\"b\":{},\"c\":{}}}],[\"help\",{\"_index\":15,\"title\":{\"c\":{}},\"body\":{}}],[\"javascript\",{\"_index\":21,\"title\":{\"d\":{}},\"body\":{\"d\":{}}}],[\"kill\",{\"_index\":2,\"title\":{\"a\":{}},\"body\":{\"a\":{}}}],[\"last\",{\"_index\":19,\"title\":{},\"body\":{\"c\":{}}}],[\"miss\",{\"_index\":16,\"title\":{},\"body\":{\"c\":{}}}],[\"mr\",{\"_index\":0,\"title\":{\"a\":{}},\"body\":{\"a\":{}}}],[\"mustard\",{\"_index\":4,\"title\":{\"a\":{}},\"body\":{\"a\":{}}}],[\"nice\",{\"_index\":8,\"title\":{},\"body\":{\"a\":{}}}],[\"object\",{\"_index\":22,\"title\":{},\"body\":{\"d\":{}}}],[\"offic\",{\"_index\":18,\"title\":{},\"body\":{\"c\":{}}}],[\"plant\",{\"_index\":12,\"title\":{\"b\":{}},\"body\":{\"b\":{},\"c\":{}}}],[\"plumb\",{\"_index\":10,\"title\":{\"b\":{}},\"body\":{\"b\":{},\"c\":{}}}],[\"professor\",{\"_index\":13,\"title\":{\"c\":{}},\"body\":{\"b\":{},\"c\":{}}}],[\"properti\",{\"_index\":25,\"title\":{},\"body\":{\"d\":{}}}],[\"scarlett\",{\"_index\":14,\"title\":{\"c\":{}},\"body\":{\"c\":{}}}],[\"special\",{\"_index\":23,\"title\":{},\"body\":{\"d\":{}}}],[\"studi\",{\"_index\":5,\"title\":{},\"body\":{\"a\":{},\"b\":{}}}],[\"veri\",{\"_index\":7,\"title\":{},\"body\":{\"a\":{}}}],[\"water\",{\"_index\":11,\"title\":{\"b\":{}},\"body\":{\"c\":{}}}],[\"week\",{\"_index\":20,\"title\":{},\"body\":{\"c\":{}}}]],\"pipeline\":[\"stemmer\"]}";

        [Fact]
        public async Task CanSerializeAndDeserializeIndex()
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

            string serializedIndex = JsonSerializer.Serialize(idx);
            // We want strict compatibility of index serialization with lunr.js
            Assert.Equal(_lunrJsSerializedIndex, serializedIndex);
            //Index deserializedIndex = JsonSerializer.Deserialize<Index>(serializedIndex);

            //string serializedResults = JsonSerializer.Serialize(
            //    await idx.Search("green").ToList());
            //string serializedResultsFromSerializedAndDeserializedIndex
            //    = JsonSerializer.Serialize(
            //        await deserializedIndex.Search("green").ToList());

            //Assert.Equal(serializedResults, serializedResultsFromSerializedAndDeserializedIndex);
        }
    }
}
