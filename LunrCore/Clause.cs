using System;
using System.Collections.Generic;
using System.Linq;

namespace Lunr
{
    /// <summary>
    /// A single clause in a `Query` contains a term and details on how to
    /// match that term against an `Index`.
    /// </summary>
    public class Clause
    {
        public static readonly Clause Empty = new Clause("");

        /// <summary>
        /// Builds a new clause.
        /// </summary>
        /// <param name="term">The term to search for.</param>
        /// <param name="boost">Any boost that should be applied when matching this clause.</param>
        /// <param name="editDistance">Whether the term should have fuzzy matching applied, and how fuzzy the match should be.</param>
        /// <param name="usePipeline">Whether the term should be passed through the search pipeline.</param>
        /// <param name="wildcard">Whether the term should have wildcards appended or prepended.</param>
        /// <param name="presence">The terms presence in any matching documents.</param>
        /// <param name="fields">The fields in an index this clause should be matched against.</param>
        public Clause(
            string term = "",
            double boost = 1,
            int editDistance = 0,
            bool usePipeline = true,
            QueryWildcard wildcard = QueryWildcard.None,
            QueryPresence presence = QueryPresence.Optional,
            IEnumerable<Field>? fields = null!)
        {
            Fields = fields ?? Array.Empty<Field>();
            Boost = boost;
            EditDistance = editDistance;
            UsePipeline = usePipeline;
            Wildcard = wildcard;
            Presence = presence;
            Term = ((wildcard & QueryWildcard.Leading) != 0 && (term[0] != Query.Wildcard) ? "*" : "") +
                term +
                ((wildcard & QueryWildcard.Trailing) != 0 && (term[term.Length - 1] != Query.Wildcard) ? "*" : "");
        }

        /// <summary>
        /// Builds a new clause.
        /// </summary>
        /// <param name="term">The term to search for.</param>
        /// <param name="fields">The fields in an index this clause should be matched against.</param>
        /// <param name="boost">Any boost that should be applied when matching this clause.</param>
        /// <param name="editDistance">Whether the term should have fuzzy matching applied, and how fuzzy the match should be.</param>
        /// <param name="usePipeline">Whether the term should be passed through the search pipeline.</param>
        /// <param name="wildcard">Whether the term should have wildcards appended or prepended.</param>
        /// <param name="presence">The terms presence in any matching documents.</param>
        public Clause(
            string term = "",
            double boost = 1,
            int editDistance = 0,
            bool usePipeline = true,
            QueryWildcard wildcard = QueryWildcard.None,
            QueryPresence presence = QueryPresence.Optional,
            params Field[] fields)
            : this(
                  term,
                  boost,
                  editDistance,
                  usePipeline,
                  wildcard,
                  presence,
                  (IEnumerable<Field>)fields) { }

        /// <summary>
        /// Builds a new clause.
        /// </summary>
        /// <param name="term">The term to search for.</param>
        /// <param name="fields">The fields in an index this clause should be matched against.</param>
        /// <param name="boost">Any boost that should be applied when matching this clause.</param>
        /// <param name="editDistance">Whether the term should have fuzzy matching applied, and how fuzzy the match should be.</param>
        /// <param name="usePipeline">Whether the term should be passed through the search pipeline.</param>
        /// <param name="wildcard">Whether the term should have wildcards appended or prepended.</param>
        /// <param name="presence">The terms presence in any matching documents.</param>
        public Clause(
            string term = "",
            double boost = 1,
            int editDistance = 0,
            bool usePipeline = true,
            QueryWildcard wildcard = QueryWildcard.None,
            QueryPresence presence = QueryPresence.Optional,
            params string[] fields)
            : this(
                  term,
                  boost,
                  editDistance,
                  usePipeline,
                  wildcard,
                  presence,
                  fields.Select(s => new Field<string>(s)))
        { }

        /// <summary>
        /// The fields in an index this clause should be matched against.
        /// </summary>
        public IEnumerable<Field> Fields { get; }

        /// <summary>
        /// Any boost that should be applied when matching this clause.
        /// </summary>
        public double Boost { get; }

        /// <summary>
        /// Whether the term should have fuzzy matching applied, and how fuzzy the match should be.
        /// </summary>
        public int EditDistance { get; }

        /// <summary>
        /// Whether the term should be passed through the search pipeline.
        /// </summary>
        public bool UsePipeline { get; }

        /// <summary>
        /// Whether the term should have wildcards appended or prepended.
        /// </summary>
        public QueryWildcard Wildcard { get; }

        /// <summary>
        /// The terms presence in any matching documents.
        /// </summary>
        public QueryPresence Presence { get; }

        /// <summary>
        /// The term to search for.
        /// </summary>
        public string Term { get; }

        /// <summary>
        /// Creates a clone of this clause with the specified term.
        /// </summary>
        /// <param name="term">The new term.</param>
        /// <returns>the new clause.</returns>
        public Clause WithTerm(string term)
            => new Clause(term, Boost, EditDistance, UsePipeline, Wildcard, Presence, Fields);

        /// <summary>
        /// Creates a clone of this clause with the specified presence.
        /// </summary>
        /// <param name="presence">The new presence.</param>
        /// <returns>the new clause.</returns>
        public Clause WithPresence(QueryPresence presence)
            => new Clause(Term, Boost, EditDistance, UsePipeline, Wildcard, presence, Fields);

        /// <summary>
        /// Creates a clone of this clause with the specified edit distance.
        /// </summary>
        /// <param name="editDistance">The new edit distance.</param>
        /// <returns>the new clause.</returns>
        public Clause WithEditDistance(int editDistance)
            => new Clause(Term, Boost, editDistance, UsePipeline, Wildcard, Presence, Fields);

        /// <summary>
        /// Creates a clone of this clause with the specified boost.
        /// </summary>
        /// <param name="boost">The new boost.</param>
        /// <returns>the new clause.</returns>
        public Clause WithBoost(double boost)
            => new Clause(Term, boost, EditDistance, UsePipeline, Wildcard, Presence, Fields);

        /// <summary>
        /// Creates a clone of this clause with the specified pipeline usage.
        /// </summary>
        /// <param name="usePipeline">The new pipeline usage.</param>
        /// <returns>the new clause.</returns>
        public Clause WithUsePipeline(bool usePipeline)
            => new Clause(Term, Boost, EditDistance, usePipeline, Wildcard, Presence, Fields);

        /// <summary>
        /// Creates a clone of this clause with the specified list of fields appended.
        /// </summary>
        /// <param name="fields">The list of fields to append.</param>
        /// <returns>the new clause.</returns>
        public Clause WithFields(IEnumerable<Field> fields)
            => new Clause(Term, Boost, EditDistance, UsePipeline, Wildcard, Presence, Fields.Concat(fields).ToArray());

        /// <summary>
        /// Creates a clone of this clause with the specified list of fields appended.
        /// </summary>
        /// <param name="fields">The list of fields to append.</param>
        /// <returns>the new clause.</returns>
        public Clause WithFields(params Field[] fields)
            => new Clause(Term, Boost, EditDistance, UsePipeline, Wildcard, Presence, Fields.Concat(fields).ToArray());
    }
}
