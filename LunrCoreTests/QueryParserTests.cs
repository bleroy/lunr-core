using Lunr;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LunrCoreTests
{
    public class QueryParserTests
    {
        [Fact]
        public void ParseSingleTerm()
        {
            IList<Clause> clauses = Parse("foo");

            Assert.Single(clauses);
            
            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(
                new[] { "title", "body" },
                clause.Fields.Select(f => f.Name));
            Assert.Equal(QueryPresence.Optional, clause.Presence);
            Assert.True(clause.UsePipeline);
        }

        [Fact]
        public void ParseSingleTermUppercase()
        {
            IList<Clause> clauses = Parse("FOO");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(
                new[] { "title", "body" },
                clause.Fields.Select(f => f.Name));
            Assert.Equal(QueryPresence.Optional, clause.Presence);
            Assert.True(clause.UsePipeline);
        }

        [Fact]
        public void ParseSingleTermWithWildcard()
        {
            IList<Clause> clauses = Parse("fo*");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("fo*", clause.Term);
            Assert.Equal(
                new[] { "title", "body" },
                clause.Fields.Select(f => f.Name));
            Assert.Equal(QueryPresence.Optional, clause.Presence);
            Assert.False(clause.UsePipeline);
        }

        [Fact]
        public void ParseMultipleTerms()
        {
            IList<Clause> clauses = Parse("foo bar");

            Assert.Equal(2, clauses.Count);

            Assert.Equal("foo", clauses[0].Term);
            Assert.Equal("bar", clauses[1].Term);
        }

        [Fact]
        public void ParseMultipleTermsWithPresence()
        {
            IList<Clause> clauses = Parse("+foo +bar");

            Assert.Equal(2, clauses.Count);

            Clause fooClause = clauses[0];
            Assert.Equal("foo", fooClause.Term);
            Assert.Equal(QueryPresence.Required, fooClause.Presence);

            Clause barClause = clauses[1];
            Assert.Equal("bar", barClause.Term);
            Assert.Equal(QueryPresence.Required, barClause.Presence);
        }

        [Fact]
        public void ParseMultipleTermsWithEditDistanceAndPresence()
        {
            IList<Clause> clauses = Parse("foo~10 +bar");

            Assert.Equal(2, clauses.Count);

            Clause fooClause = clauses[0];
            Assert.Equal("foo", fooClause.Term);
            Assert.Equal(QueryPresence.Optional, fooClause.Presence);
            Assert.Equal(10, fooClause.EditDistance);

            Clause barClause = clauses[1];
            Assert.Equal("bar", barClause.Term);
            Assert.Equal(QueryPresence.Required, barClause.Presence);
            Assert.Equal(0, barClause.EditDistance);
        }

        [Fact]
        public void ParseMultipleTermsWithBoostAndPresence()
        {
            IList<Clause> clauses = Parse("foo^10 +bar");

            Assert.Equal(2, clauses.Count);

            Clause fooClause = clauses[0];
            Assert.Equal("foo", fooClause.Term);
            Assert.Equal(QueryPresence.Optional, fooClause.Presence);
            Assert.Equal(10, fooClause.Boost);

            Clause barClause = clauses[1];
            Assert.Equal("bar", barClause.Term);
            Assert.Equal(QueryPresence.Required, barClause.Presence);
            Assert.Equal(0, barClause.EditDistance);
        }

        private static IList<Clause> Parse(string q)
        {
            var query = new Query(
                new Field<string>("title"),
                new Field<string>("body"));
            var parser = new QueryParser(q, query);
            
            parser.Parse();

            return query.Clauses;
        }
    }
}
