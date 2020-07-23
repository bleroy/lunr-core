using Lunr;
using System.Collections.Generic;
using Xunit;

namespace LunrCoreTests
{
    public class MatchDataTests
    {
        [Fact]
        public void MatchDataCombines()
        {
            var match = new MatchData(
                "foo",
                "title",
                new Metadata {
                    { "position", new List<object> { 1 } }
                });
            match.Combine(new MatchData(
                "bar",
                "title",
                new Metadata {
                    { "position", new List<object> { 2 } }
                }));
            match.Combine(new MatchData(
                "baz",
                "body",
                new Metadata {
                    { "position", new List<object> { 3 } }
                }));
            match.Combine(new MatchData(
                "baz",
                "body",
                new Metadata {
                    { "position", new List<object> { 4 } }
                }));

            Assert.Equal(
                new[] { "foo", "bar", "baz" },
                match.Posting.Keys);

            Assert.Equal(
                new object[] { 1 },
                match.Posting["foo"]["title"]["position"]);
            Assert.Equal(
                new object[] { 2 },
                match.Posting["bar"]["title"]["position"]);
            Assert.Equal(
                new object[] { 3, 4 },
                match.Posting["baz"]["body"]["position"]);
        }

        [Fact]
        public void CombineDoesntMutateDataSource()
        {
            var metadata = new Metadata
            {
                { "foo", new object[] { 1 } }
            };
            var matchData1 = new MatchData("foo", "title", metadata);
            var matchData2 = new MatchData("foo", "title", metadata);

            matchData1.Combine(matchData2);

            Assert.Equal(
                new object[] { 1 },
                metadata["foo"]);
        }
    }
}
