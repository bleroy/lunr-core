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
        internal Query(params Field[] allFields)
        {
            AllFields = allFields;
        }

        /// <summary>
        /// Builds a new query.
        /// </summary>
        /// <param name="allFields">An array of all available fields in a `Index`.</param>
        internal Query(IEnumerable<Field> allFields)
        {
            AllFields = new List<Field>(allFields);
        }

        public static readonly char Wildcard = '*';

        /// <summary>
        /// An array of all available fields.
        /// </summary>
        public IList<Field> AllFields { get; }

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
        /// Adds a`Clause` to this query.
        /// Unless the clause contains the fields to be matched all fields will be matched.
        /// In addition, a default boost of 1 is applied to the clause.
        /// </summary>
        /// <param name="clause">The clause to add to this query.</param>
        /// <returns>The query.</returns>
        public Query AddClause(Clause clause)
        {
            if (!clause.Fields.Any())
            {
                Clauses.Add(clause.WithFields(AllFields));
            }
            else
            {
                Clauses.Add(clause);
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
        /// <returns>The query.</returns>
        public Query AddTerm(string term) => AddClause(new Clause(term));
    }
}
