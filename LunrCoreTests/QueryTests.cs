using Lunr;
using System.Linq;
using Xunit;

namespace LunrCoreTests
{
    public class QueryTests
    {
        private static readonly string[] _allFields = new[] { "title", "body" };

        [Fact]
        public void SingleStringTerm()
        {
            Query query = new Query(_allFields).AddTerm("foo");

            Assert.Equal("foo", query.Clauses.Single().Term);
        }

        [Fact]
        public void SingleTokenTerm()
        {
            Query query = new Query(_allFields).AddTerm(new Token("foo"));

            Assert.Equal("foo", query.Clauses.Single().Term);
        }

        [Fact]
        public void MultipleStringTerms()
        {
            Query query = new Query(_allFields).AddTerms("foo", "bar");

            Assert.Equal(
                new[] { "foo", "bar" },
                query.Clauses.Select(c => c.Term));
            Assert.True(query.Clauses.All(c => c.UsePipeline));
        }

        [Fact]
        public void MultipleStringTermsWithOptions()
        {
            Query query = new Query(_allFields)
                .AddTerms(new Clause(usePipeline: false), "foo", "bar");

            Assert.Equal(
                new[] { "foo", "bar" },
                query.Clauses.Select(c => c.Term));
            Assert.True(query.Clauses.All(c => !c.UsePipeline));
        }

        [Fact]
        public void MultipleTokenTerms()
        {
            Query query = new Query(_allFields)
                .AddTerms(new Tokenizer().Tokenize("foo bar"));

            Assert.Equal(
                new[] { "foo", "bar" },
                query.Clauses.Select(c => c.Term));
        }

        [Fact]
        public void ClauseDefaults()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(term: "foo"));

            Clause clause = query.Clauses.Single();

            Assert.Equal("foo", clause.Term);
            Assert.Equal(_allFields, clause.Fields);
            Assert.Equal(1, clause.Boost);
            Assert.True(clause.UsePipeline);
        }

        [Fact]
        public void SpecifiedClause()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "foo",
                    boost: 10,
                    fields: new[] { "title" },
                    usePipeline: false));

            Clause clause = query.Clauses.Single();

            Assert.Equal("foo", clause.Term);
            Assert.Equal("title", clause.Fields.Single());
            Assert.Equal(10, clause.Boost);
            Assert.False(clause.UsePipeline);
        }

        [Fact]
        public void NoWildcards()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "foo",
                    wildcard: QueryWildcard.None));

            Clause clause = query.Clauses.Single();

            Assert.Equal("foo", clause.Term);
        }

        [Fact]
        public void LeadingWildcard()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "foo",
                    wildcard: QueryWildcard.Leading));

            Clause clause = query.Clauses.Single();

            Assert.Equal("*foo", clause.Term);
        }

        [Fact]
        public void TrailingWildcard()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "foo",
                    wildcard: QueryWildcard.Trailing));

            Clause clause = query.Clauses.Single();

            Assert.Equal("foo*", clause.Term);
        }

        [Fact]
        public void LeadingAndTrailingWildcards()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "foo",
                    wildcard: QueryWildcard.Both));

            Clause clause = query.Clauses.Single();

            Assert.Equal("*foo*", clause.Term);
        }

        [Fact]
        public void ExistingWildcards()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "*foo*",
                    wildcard: QueryWildcard.Both));

            Clause clause = query.Clauses.Single();

            Assert.Equal("*foo*", clause.Term);
        }

        [Fact]
        public void AllProhibitedIsNegated()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "foo",
                    presence: QueryPresence.Prohibited))
                .AddClause(new Clause(
                    term: "bar",
                    presence: QueryPresence.Prohibited));

            Assert.True(query.IsNegated);
        }

        [Fact]
        public void SomeProhibitedIsNotNegated()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "foo",
                    presence: QueryPresence.Prohibited))
                .AddClause(new Clause(
                    term: "bar",
                    presence: QueryPresence.Required));

            Assert.False(query.IsNegated);
        }

        [Fact]
        public void NoneProhibitedIsNotNegated()
        {
            Query query = new Query(_allFields)
                .AddClause(new Clause(
                    term: "foo",
                    presence: QueryPresence.Optional))
                .AddClause(new Clause(
                    term: "bar",
                    presence: QueryPresence.Required));

            Assert.False(query.IsNegated);
        }
    }
}
