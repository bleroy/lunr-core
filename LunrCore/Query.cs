using System.Collections.Generic;
using System.Linq;

namespace Lunr
{
    /// <summary>
    /// A `Query` provides a programmatic way of defining queries to be performed
    /// against an `Index`.
    ///
    /// Prefer constructing a `Query` using the `Index.Query` method
    /// so the query object is pre -initialized with the right index fields.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Builds a new query.
        /// </summary>
        /// <param name="allFields">An array of all available fields in a `Index`.</param>
        public Query(params string[] allFields)
        {
            AllFields = allFields;
        }

        /// <summary>
        /// Builds a new query.
        /// </summary>
        /// <param name="allFields">An array of all available fields in a `Index`.</param>
        internal Query(IEnumerable<string> allFields)
        {
            AllFields = new List<string>(allFields);
        }

        public static readonly char Wildcard = '*';

        /// <summary>
        /// An array of all available fields.
        /// </summary>
        public IList<string> AllFields { get; }

        /// <summary>
        /// An list of query clauses.
        /// </summary>
        public IList<Clause> Clauses { get; } = new List<Clause>();

        /// <summary>
        /// A negated query is one in which every clause has a presence of
        /// prohibited.These queries require some special processing to return
        /// the expected results.
        /// </summary>
        public bool IsNegated => Clauses.All(clause => clause.Presence == QueryPresence.Prohibited);

        /// <summary>
        /// Adds a `Clause` to this query.
        /// Unless the clause contains the fields to be matched all fields will be matched.
        /// In addition, a default boost of 1 is applied to the clause.
        /// </summary>
        /// <param name="clause">The clause to add to this query.</param>
        /// <returns>The query.</returns>
        public Query AddClause(Clause clause)
        {
            Clauses.Add(!clause.Fields.Any() ? clause.WithFields(AllFields) : clause);
            return this;
        }

        /// <summary>
        /// Adds multiple terms using copies of a single clause to this query.
        /// Unless the clause contains the fields to be matched all fields will be matched.
        /// In addition, a default boost of 1 is applied to the clause.
        /// </summary>
        /// <param name="clause">The clause to copy with terms to add to this query.</param>
        /// <param name="terms">The terms to add with common parameters defined by the clause.</param>
        /// <returns>The query.</returns>
        public Query AddTerms(Clause clause, params string[] terms)
        {
            foreach(string term in terms)
            {
                AddClause(clause.WithTerm(term));
            }
            return this;
        }

        /// <summary>
        /// Adds a term to the current query, under the covers this will create a `Clause`
        /// to the list of clauses that make up this query.
        /// 
        /// The term is used as is, i.e.no tokenization will be performed by this method.
        /// Instead, conversion to a token or token-like string should be done before calling this method.
        /// </summary>
        /// <param name="term">The term to add to the query.</param>
        /// <param name="boost">An optional boost for the term.</param>
        /// <param name="editDistance">The maximum edit distance from the term.</param>
        /// <param name="usePipeline">Set to false to bypass the pipeline.</param>
        /// <param name="wildcard">An optional wildcard.</param>
        /// <param name="presence">The type of presence for this term.</param>
        /// <param name="fields">An optional list of fields to look for the term in.</param>
        /// <returns>The query.</returns>
        public Query AddTerm(
            string term = "",
            double boost = 1,
            int editDistance = 0,
            bool usePipeline = true,
            QueryWildcard wildcard = QueryWildcard.None,
            QueryPresence presence = QueryPresence.Optional,
            IEnumerable<string>? fields = null)
            => AddClause(new Clause(term, boost, editDistance, usePipeline, wildcard, presence, fields));

        /// <summary>
        /// Adds multiple terms to the current query, under the covers this will create a `Clause`
        /// to the list of clauses that make up this query.
        ///
        /// The term is used as is, i.e.no tokenization will be performed by this method.
        /// Instead, conversion to a token or token-like string should be done before calling this method.
        /// </summary>
        /// <param name="terms">The terms to add to the query.</param>
        /// <returns>The query.</returns>
        public Query AddTerms(params string[] terms)
            => AddTerms((IEnumerable<string>)terms);

        /// <summary>
        /// Adds multiple terms to the current query, under the covers this will create a `Clause`
        /// to the list of clauses that make up this query.
        ///
        /// The term is used as is, i.e.no tokenization will be performed by this method.
        /// Instead, conversion to a token or token-like string should be done before calling this method.
        /// </summary>
        /// <param name="terms">The terms to add to the query.</param>
        /// <returns>The query.</returns>
        public Query AddTerms(IEnumerable<string> terms)
        {
            foreach (string term in terms)
            {
                AddTerm(term);
            }
            return this;
        }

        /// <summary>
        /// Adds multiple terms to the current query, under the covers this will create a `Clause`
        /// to the list of clauses that make up this query.
        ///
        /// The term is used as is, i.e.no tokenization will be performed by this method.
        /// Instead, conversion to a token or token-like string should be done before calling this method.
        /// </summary>
        /// <param name="terms">The terms to add to the query.</param>
        /// <returns>The query.</returns>
        public Query AddTerms(IEnumerable<Token> terms)
            => AddTerms(terms.Select(t => t.String));
    }
}
