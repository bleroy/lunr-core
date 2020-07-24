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
            CheckBothFields(clause);
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
            CheckBothFields(clause);
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
            CheckBothFields(clause);
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

        [Fact]
        public void FieldWithoutATerm()
        {
            Assert.Throws<QueryParserException>(() =>
            {
                Parse("title:");
            });
        }

        [Fact]
        public void UnknownField()
        {
            Assert.Throws<QueryParserException>(() =>
            {
                Parse("unknown:foo");
            });
        }

        [Fact]
        public void TermWithField()
        {
            IList<Clause> clauses = Parse("title:foo");

            Assert.Single(clauses);
            Assert.Equal(new[] { "title" }, clauses[0].Fields);
            Assert.Equal("foo", clauses[0].Term);
        }

        [Fact]
        public void UppercaseFieldWithUppercaseTerm()
        {
            var query = new Query("TITLE");
            var parser = new QueryParser("TITLE:FOO", query);

            parser.Parse();

            var clauses = query.Clauses;

            Assert.Single(clauses);
            Assert.Equal("foo", clauses[0].Term);
            Assert.Equal("TITLE", clauses[0].Fields.Single());
        }

        [Fact]
        public void ParseMultipleTermsScopedToDifferentFields()
        {
            IList<Clause> clauses = Parse("title:foo body:bar");

            Assert.Equal(2, clauses.Count);

            Clause fooClause = clauses[0];
            Assert.Equal("foo", fooClause.Term);
            Assert.Equal("title", fooClause.Fields.Single());

            Clause barClause = clauses[1];
            Assert.Equal("bar", barClause.Term);
            Assert.Equal("body", barClause.Fields.Single());
        }

        [Fact]
        public void ParseSingleTermWithEditDistance()
        {
            IList<Clause> clauses = Parse("foo~2");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(2, clause.EditDistance);
            CheckBothFields(clause);
        }

        [Fact]
        public void ParseMultipleTermsWithEditDistance()
        {
            IList<Clause> clauses = Parse("foo~2 bar~3");

            Assert.Equal(2, clauses.Count);

            Clause fooClause = clauses[0];
            Assert.Equal("foo", fooClause.Term);
            Assert.Equal(2, fooClause.EditDistance);
            CheckBothFields(fooClause);

            Clause barClause = clauses[1];
            Assert.Equal("bar", barClause.Term);
            Assert.Equal(3, barClause.EditDistance);
            CheckBothFields(barClause);
        }

        [Fact]
        public void ParseSingleTermScopedToFieldWithEditDistance()
        {
            IList<Clause> clauses = Parse("title:foo~2");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(2, clause.EditDistance);
            Assert.Equal("title", clause.Fields.Single());
        }

        [Fact]
        public void NonNumericEditDistance()
        {
            Assert.Throws<QueryParserException>(() =>
            {
                Parse("foo~a");
            });
        }

        [Fact]
        public void EditDistanceWithoutATerm()
        {
            Assert.Throws<QueryParserException>(() =>
            {
                Parse("~2");
            });
        }

        [Fact]
        public void ParseSingleTermWithBoost()
        {
            IList<Clause> clauses = Parse("foo^2");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(2, clause.Boost);
            CheckBothFields(clause);
        }

        [Fact]
        public void NonNumericBoost()
        {
            Assert.Throws<QueryParserException>(() =>
            {
                Parse("foo^a");
            });
        }

        [Fact]
        public void BoostWithoutATerm()
        {
            Assert.Throws<QueryParserException>(() =>
            {
                Parse("^2");
            });
        }

        [Fact]
        public void ParseMultipleTermsWithBoost()
        {
            IList<Clause> clauses = Parse("foo^2 bar^3");

            Assert.Equal(2, clauses.Count);

            Clause fooClause = clauses[0];
            Assert.Equal("foo", fooClause.Term);
            Assert.Equal(2, fooClause.Boost);
            CheckBothFields(fooClause);

            Clause barClause = clauses[1];
            Assert.Equal("bar", barClause.Term);
            Assert.Equal(3, barClause.Boost);
            CheckBothFields(barClause);
        }

        [Fact]
        public void ParseSingleTermScopedByFieldWithBoost()
        {
            IList<Clause> clauses = Parse("title:foo^2");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(2, clause.Boost);
            Assert.Equal("title", clause.Fields.Single());
        }

        [Fact]
        public void ParseSingleTermWithPresenceRequired()
        {
            IList<Clause> clauses = Parse("+foo");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(QueryPresence.Required, clause.Presence);
            Assert.Equal(1, clause.Boost);
            CheckBothFields(clause);
        }

        [Fact]
        public void ParseSingleTermWithPresenceProhibited()
        {
            IList<Clause> clauses = Parse("-foo");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(QueryPresence.Prohibited, clause.Presence);
            Assert.Equal(1, clause.Boost);
            CheckBothFields(clause);
        }

        [Fact]
        public void ParseSingleTermScopedWithFieldWithPresenceRequired()
        {
            IList<Clause> clauses = Parse("+title:foo");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(QueryPresence.Required, clause.Presence);
            Assert.Equal(1, clause.Boost);
            Assert.Equal("title", clause.Fields.Single());
        }

        [Fact]
        public void ParseSingleTermScopedWithFieldWithPresenceProhibited()
        {
            IList<Clause> clauses = Parse("-title:foo");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(QueryPresence.Prohibited, clause.Presence);
            Assert.Equal(1, clause.Boost);
            Assert.Equal("title", clause.Fields.Single());
        }

        [Fact]
        public void ParseSingleTermWithBoostAndEditDistance()
        {
            IList<Clause> clauses = Parse("foo^2~3");

            Assert.Single(clauses);

            Clause clause = clauses.First();
            Assert.Equal("foo", clause.Term);
            Assert.Equal(3, clause.EditDistance);
            Assert.Equal(2, clause.Boost);
            CheckBothFields(clause);
        }

        private static IList<Clause> Parse(string q)
        {
            var query = new Query("title", "body");
            var parser = new QueryParser(q, query);
            
            parser.Parse();

            return query.Clauses;
        }

        private static void CheckBothFields(Clause clause)
        {
            Assert.Equal(
                new[] { "title", "body" },
                clause.Fields);
        }
    }
}
