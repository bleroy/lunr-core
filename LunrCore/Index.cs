using Lunr.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Lunr
{
    /// <summary>
    /// An index contains the built index of all documents and provides a query interface
    /// to the index.
    ///
    /// Instances of `Index` can't be created, instead `Builder` should be used to construct new indexes,
    /// or `Index.Load` should be used to load previously built and serialized indexes.
    /// </summary>
    [JsonConverter(typeof(IndexJsonConverter))]
    public class Index
    {
        /// <summary>
        /// Constructs a new index.
        /// </summary>
        /// <param name="invertedIndex">An index of term/field to document reference.</param>
        /// <param name="fieldVectors">Field vectors</param>
        /// <param name="tokenSet">A set of all corpus tokens.</param>
        /// <param name="fields">The names of indexed document fields.</param>
        /// <param name="pipeline">The pipeline to use for search terms.</param>
        internal Index(
            InvertedIndex invertedIndex,
            IDictionary<string, Vector> fieldVectors,
            TokenSet tokenSet,
            IEnumerable<string> fields,
            Pipeline pipeline)
        {
            InvertedIndex = invertedIndex;
            FieldVectors = fieldVectors;
            TokenSet = tokenSet;
            Fields = fields;
            Pipeline = pipeline;
        }

        /// <summary>
        /// A convenience function for configuring and constructing
        /// a new lunr Index.
        /// 
        /// A `Builder` instance is created and the pipeline setup
        /// with a trimmer, stop word filter and stemmer.
        /// 
        /// This builder object is yielded to the configuration function
        /// that is passed as a parameter, allowing the list of fields
        /// and other builder parameters to be customized.
        /// 
        /// All documents _must_ be added within the passed config function.
        /// </summary>
        /// <example>
        /// var idx = Index.Build(async builder =>
        /// {
        ///      builder
        ///         .AddField("title")
        ///         .AddField("body");
        /// 
        ///      builder.ReferenceField = "id";
        /// 
        ///      foreach(Document doc in documents)
        ///      {
        ///          builder.add(doc);
        ///      }
        /// });
        /// </example>
        /// <param name="trimmer">An optional trimmer.</param>
        /// <param name="stopWordFilter">An optional stop word filter. Default is English.</param>
        /// <param name="stemmer">An optional stemmer. Default is English.</param>
        /// <param name="config">A Configuration function.</param>
        /// <param name="tokenizer">An optional tokenizer.</param>
        /// <param name="registry">An optional pipeline function registry. The default has the trimmer, stop word filter and stemmer.</param>
        /// <param name="indexingPipeline">An optional indexing pipeline. The default has the trimmer, stop word filter and stemmer.</param>
        /// <param name="searchPipeline">An optional search pipeline. The default has the stemmer.</param>
        /// <param name="fields">An optional list of fields.</param>
        /// <returns>The index.</returns>
        public static async Task<Index> Build(
            Func<Builder, Task>? config = null,
            TrimmerBase? trimmer = null,
            StopWordFilterBase? stopWordFilter = null,
            StemmerBase? stemmer = null,
            Tokenizer? tokenizer = null,
            PipelineFunctionRegistry? registry = null,
            IEnumerable<string>? indexingPipeline = null,
            IEnumerable<string>? searchPipeline = null,
            params Field[] fields)
        {
            Pipeline.Function trimmerFunction = (trimmer ?? new Trimmer()).FilterFunction;
            Pipeline.Function filterFunction = (stopWordFilter ?? new EnglishStopWordFilter()).FilterFunction;
            Pipeline.Function stemmerFunction = (stemmer ?? new EnglishStemmer()).StemmerFunction;
            registry ??= new PipelineFunctionRegistry();
            registry.Add("trimmer", trimmerFunction);
            registry.Add("stopWordFilter", filterFunction);
            registry.Add("stemmer", stemmerFunction);

            Pipeline idxPipeline = indexingPipeline is null ?
                new Pipeline(registry, trimmerFunction, filterFunction, stemmerFunction) :
                new Pipeline(registry, indexingPipeline.Select(function => registry[function]).ToArray());
            Pipeline srchPipeline = searchPipeline is null ?
                new Pipeline(registry, stemmerFunction) :
                new Pipeline(registry, searchPipeline.Select(function => registry[function]).ToArray());

            var builder = new Builder(
                indexingPipeline: idxPipeline,
                searchPipeline: srchPipeline,
                tokenizer: tokenizer ?? new Tokenizer(),
                fields: fields);

            if (config != null)
            {
                await config(builder).ConfigureAwait(false);
            }

            return builder.Build();
        }

        /// <summary>
        /// An index of term/field to document reference.
        /// </summary>
        public InvertedIndex InvertedIndex { get; }

        /// <summary>
        /// Field vectors.
        /// </summary>
        public IDictionary<string, Vector> FieldVectors { get; }

        /// <summary>
        /// A set of all corpus tokens.
        /// </summary>
        public TokenSet TokenSet { get; }

        /// <summary>
        /// The names of indexed document fields.
        /// </summary>
        public IEnumerable<string> Fields { get; }

        /// <summary>
        /// The pipeline to use for search terms.
        /// </summary>
        public Pipeline Pipeline { get; }

        /// <summary>
        /// Performs a search against the index using lunr query syntax.
        ///
        /// Results will be returned sorted by their score, the most relevant results
        /// will be returned first.  For details on how the score is calculated, please see
        /// the [scoring guide](https://lunrjs.com/guides/searching.html#scoring|guide).
        ///
        /// For more programmatic querying use `Index.Query`.
        /// </summary>
        /// <param name="queryString">A string containing a lunr query.</param>
        /// <param name="culture">The culture to use to parse the query.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The list of results.</returns>
        public async IAsyncEnumerable<Result> Search(
            string queryString,
            CultureInfo culture,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (Result result in Query(
                query => new QueryParser(queryString, query, culture).Parse(),
                cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Performs a search against the index using lunr query syntax.
        ///
        /// Results will be returned sorted by their score, the most relevant results
        /// will be returned first.  For details on how the score is calculated, please see
        /// the [scoring guide](https://lunrjs.com/guides/searching.html#scoring|guide).
        ///
        /// For more programmatic querying use `Index.Query`.
        /// </summary>
        /// <param name="queryString">A string containing a lunr query.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The list of results.</returns>
        public async IAsyncEnumerable<Result> Search(
            string queryString,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (Result result in Search(queryString, CultureInfo.CurrentCulture, cancellationToken))
            {
                yield return result;
            }
        }

        /// <summary>
        /// Performs a search against the index using lunr query syntax.
        ///
        /// Results will be returned sorted by their score, the most relevant results
        /// will be returned first.  For details on how the score is calculated, please see
        /// the [scoring guide](https://lunrjs.com/guides/searching.html#scoring|guide).
        ///
        /// For more programmatic querying use `Index.Query`.
        /// </summary>
        /// <param name="queryString">A string containing a lunr query.</param>
        /// <param name="culture">The culture to use to parse the query.</param>
        /// <returns>The list of results.</returns>
        public async IAsyncEnumerable<Result> Search(
            string queryString,
            CultureInfo culture)
        {
            await foreach (Result result in Search(queryString, culture, new CancellationToken()))
            {
                yield return result;
            }
        }

        /// <summary>
        /// Performs a search against the index using lunr query syntax.
        ///
        /// Results will be returned sorted by their score, the most relevant results
        /// will be returned first.  For details on how the score is calculated, please see
        /// the [scoring guide](https://lunrjs.com/guides/searching.html#scoring|guide).
        ///
        /// For more programmatic querying use `Index.Query`.
        /// </summary>
        /// <param name="queryString">A string containing a lunr query.</param>
        /// <returns>The list of results.</returns>
        public async IAsyncEnumerable<Result> Search(string queryString)
        {
            await foreach (Result result in Search(queryString, CultureInfo.CurrentCulture, new CancellationToken()))
            {
                yield return result;
            }
        }

        /// <summary>
        /// Performs a query against the index using the `Query` object built
        /// by the provided factory.
        ///
        /// If performing programmatic queries against the index, this method is preferred
        /// over `Index.Search` so as to avoid the additional query parsing overhead.
        ///
        /// A query object is yielded to the supplied function which should be used to
        /// express the query to be run against the index.
        /// </summary>
        /// <param name="queryFactory">A function that builds the query object that gets passed to it.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The results of the query.</returns>
        public async IAsyncEnumerable<Result> Query(
            Action<Query> queryFactory,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var results = new List<Result>();
            var query = new Query(Fields);
            var matchingFields = new Dictionary<FieldReference, MatchData>();
            var termFieldCache = new HashSet<string>();
            var requiredMatches = new Dictionary<string, ISet<string>>();
            var prohibitedMatches = new Dictionary<string, ISet<string>>();

            // To support field level boosts a query vector is created per
            // field. An empty vector is eagerly created to support negated
            // queries.
            var queryVectors = new Dictionary<string, Vector>();
            foreach (string field in Fields)
            {
                queryVectors[field] = new Vector();
            }

            queryFactory(query);

            for (int i = 0; i < query.Clauses.Count; i++)
            {
                Clause clause = query.Clauses[i];
                ISet<string> clauseMatches = Set<string>.Empty;

                // Unless the pipeline has been disabled for this term, which is
                // the case for terms with wildcards, we need to pass the clause
                // term through the search pipeline. A pipeline returns an array
                // of processed terms. Pipeline functions may expand the passed
                // term, which means we may end up performing multiple index lookups
                // for a single query term.
                await foreach (string term in clause.UsePipeline
                    ? Pipeline.RunString(
                        clause.Term,
                        new TokenMetadata
                        {
                            { "fields", clause.Fields }
                        },
                        cancellationToken)
                    : new[] { clause.Term }.ToAsyncEnumerable(cancellationToken))
                {
                    // Each term returned from the pipeline needs to use the same query
                    // clause object, e.g. the same boost and or edit distance. The
                    // simplest way to do this is to re-use the clause object but mutate
                    // its term property.
                    clause = clause.WithTerm(term);

                    // From the term in the clause we create a token set which will then
                    // be used to intersect the indexes token set to get a list of terms
                    // to lookup in the inverted index.
                    var termTokenSet = TokenSet.FromClause(clause);
                    TokenSet expandedTerms = TokenSet.Intersect(termTokenSet);

                    // If a term marked as required does not exist in the tokenSet it is
                    // impossible for the search to return any matches.We set all the field
                    // scoped required matches set to empty and stop examining any further
                    // clauses.
                    if (!expandedTerms.Any() && clause.Presence == QueryPresence.Required)
                    {
                        foreach (string field in clause.Fields)
                        {
                            if (!requiredMatches.TryGetValue(field, out var value))
                            {
                                requiredMatches.Add(field, Set<string>.Empty);
                            }
                        }

                        break;
                    }

                    foreach (string expandedTerm in expandedTerms.ToEnumeration())
                    {
                        // For each term get the posting and termIndex, this is required for building the query vector.
                        InvertedIndexEntry posting = InvertedIndex[expandedTerm];
                        int termIndex = posting.Index;

                        foreach (string field in clause.Fields)
                        {
                            // For each field that this query term is scoped by (by default
                            // all fields are in scope) we need to get all the document refs
                            // that have this term in that field.
                            //
                            // The posting is the entry in the invertedIndex for the matching
                            // term from above.
                            // For each field that this query term is scoped by (by default
                            // all fields are in scope) we need to get all the document refs
                            // that have this term in that field.
                            //
                            // The posting is the entry in the invertedIndex for the matching
                            // term from above.
                            FieldMatches fieldPosting = posting[field];
                            ICollection<string> matchingDocumentRefs = fieldPosting.Keys;
                            string termField = expandedTerm + '/' + field;
                            var matchingDocumentSet = new Set<string>(matchingDocumentRefs);

                            // if the presence of this term is required ensure that the matching
                            // documents are added to the set of required matches for this clause.
                            if (clause.Presence == QueryPresence.Required)
                            {
                                clauseMatches = clauseMatches.Union(matchingDocumentSet);

                                if (!requiredMatches.ContainsKey(field))
                                {
                                    requiredMatches.Add(field, Set<string>.Complete);
                                }
                            }

                            // if the presence of this term is prohibited ensure that the matching
                            // documents are added to the set of prohibited matches for this field,
                            // creating that set if it does not yet exist.
                            if (clause.Presence == QueryPresence.Prohibited)
                            {
                                if (!prohibitedMatches.ContainsKey(field))
                                {
                                    prohibitedMatches.Add(field, Set<string>.Empty);
                                }

                                prohibitedMatches[field] = prohibitedMatches[field].Union(matchingDocumentSet);

                                // Prohibited matches should not be part of the query vector used for
                                // similarity scoring and no metadata should be extracted so we continue
                                // to the next field.
                                continue;
                            }

                            // The query field vector is populated using the termIndex found for
                            // the term and a unit value with the appropriate boost applied.
                            // Using upsert because there could already be an entry in the vector
                            // for the term we are working with.In that case we just add the scores
                            // together.
                            queryVectors[field].Upsert(
                                termIndex,
                                clause.Boost,
                                (a, b) => a + b);

                            // If we've already seen this term, field combo then we've already collected
                            // the matching documents and metadata, no need to go through all that again.
                            if (termFieldCache.Contains(termField)) continue;

                            foreach (string matchingDocumentRef in matchingDocumentRefs)
                            {
                                // All metadata for this term/field/document triple
                                // are then extracted and collected into an instance
                                // of lunr.MatchData ready to be returned in the query
                                // results.
                                var matchingFieldRef = new FieldReference(matchingDocumentRef, field);
                                FieldMatchMetadata metadata = fieldPosting[matchingDocumentRef];
                                
                                if (!matchingFields.TryGetValue(matchingFieldRef, out MatchData? fieldMatch))
                                {
                                    matchingFields.Add(
                                        matchingFieldRef,
                                        new MatchData(expandedTerm, field, metadata));
                                }
                                else
                                {
                                    fieldMatch.Add(expandedTerm, field, metadata);
                                }
                            }

                            termFieldCache.Add(termField);
                        }
                    }
                }

                // If the presence was required we need to update the requiredMatches field sets.
                // We do this after all fields for the term have collected their matches because
                // the clause terms presence is required in _any_ of the fields not _all_ of the
                // fields.
                if (clause.Presence == QueryPresence.Required)
                {
                    foreach (string field in clause.Fields)
                    {
                        requiredMatches[field] = requiredMatches[field].Intersect(clauseMatches);
                    }
                }
            }

            // Need to combine the field scoped required and prohibited
            // matching documents into a global set of required and prohibited
            // matches.
            ISet<string> allRequiredMatches = Set<string>.Complete;
            ISet<string> allProhibitedMatches = Set<string>.Empty;

            foreach (string field in Fields)
            {
                if (requiredMatches.ContainsKey(field))
                {
                    allRequiredMatches = allRequiredMatches.Intersect(requiredMatches[field]);
                }

                if (prohibitedMatches.ContainsKey(field))
                {
                    allProhibitedMatches = allProhibitedMatches.Union(prohibitedMatches[field]);
                }
            }

            string[] matchingFieldRefs
                = matchingFields.Keys.Select(k => k.ToString()).ToArray();
            var matches = new Dictionary<string, Result>();

            // If the query is negated (contains only prohibited terms)
            // we need to get _all_ fieldRefs currently existing in the
            // index. This is only done when we know that the query is
            // entirely prohibited terms to avoid any cost of getting all
            // fieldRefs unnecessarily.
            //
            // Additionally, blank MatchData must be created to correctly
            // populate the results.
            if (query.IsNegated)
            {
                matchingFieldRefs = FieldVectors.Keys.ToArray();

                foreach (string matchingFieldRef in matchingFieldRefs)
                {
                    var fieldRef = FieldReference.FromString(matchingFieldRef);
                    matchingFields.Add(fieldRef, MatchData.Empty);
                }
            }

            foreach (string fieldRefString in matchingFieldRefs)
            {
                // Currently we have document fields that match the query, but we
                // need to return documents.The matchData and scores are combined
                // from multiple fields belonging to the same document.
                //
                // Scores are calculated by field, using the query vectors created
                // above, and combined into a final document score using addition.
                var fieldRef = FieldReference.FromString(fieldRefString);
                string docRef = fieldRef.DocumentReference;

                if (!allRequiredMatches.Contains(docRef)) continue;
                if (allProhibitedMatches.Contains(docRef)) continue;

                Vector fieldVector = FieldVectors[fieldRefString];
                double score = queryVectors[fieldRef.FieldName].Similarity(fieldVector);

                if (matches.TryGetValue(docRef, out Result? docMatch))
                {
                    docMatch.Score += score;
                    docMatch.MatchData.Combine(matchingFields[fieldRef]);
                }
                else
                {
                    var match = new Result(
                        documentReference: docRef,
                        score,
                        matchData: matchingFields[fieldRef]
                    );
                    matches.Add(docRef, match);
                    if (cancellationToken.IsCancellationRequested) yield break;
                    results.Add(match);
                }
            }

            foreach (Result match in results.OrderByDescending(r => r.Score))
            {
                yield return match;
            }
        }

        /// <summary>
        /// Performs a query against the index using the `Query` object built
        /// by the provided factory.
        ///
        /// If performing programmatic queries against the index, this method is preferred
        /// over `Index.Search` so as to avoid the additional query parsing overhead.
        ///
        /// A query object is yielded to the supplied function which should be used to
        /// express the query to be run against the index.
        /// </summary>
        /// <param name="queryFactory">A function that builds the query object that gets passed to it.</param>
        /// <returns>The results of the query.</returns>
        public async IAsyncEnumerable<Result> Query(Action<Query> queryFactory)
        {
            var cToken = new CancellationToken();
            await foreach (Result result in Query(queryFactory, cToken))
            {
                yield return result;
            }
        }

        /// <summary>
        /// Load an index from a JSON stream.
        /// </summary>
        /// <param name="utf8json">The JSON stream</param>
        /// <param name="stemmer">An optional stemmer. English is used if none is provided.</param>
        /// <param name="registry">An optional registry of pipeline functions to use to resolve the persisted pipeline.</param>
        /// <returns>The index.</returns>
        public static async Task<Index> LoadFromJsonStream(
            Stream utf8json,
            StemmerBase? stemmer = null,
            PipelineFunctionRegistry? registry = null)
        {
            Index index = (await JsonSerializer.DeserializeAsync<Index>(utf8json).ConfigureAwait(false))!;
            return ProcessDeserializedIndex(stemmer, registry, index);
        }

        /// <summary>
        /// Load an index from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string</param>
        /// <param name="stemmer">An optional stemmer. English is used if none is provided.</param>
        /// <param name="registry">An optional registry of pipeline functions to use to resolve the persisted pipeline.</param>
        /// <returns>The index.</returns>
        public static Index LoadFromJson(
            string json,
            StemmerBase? stemmer = null,
            PipelineFunctionRegistry? registry = null)
        {
            Index index = JsonSerializer.Deserialize<Index>(json)!;
            return ProcessDeserializedIndex(stemmer, registry, index);
        }

        /// <summary>
        /// Persists an index to a stream as JSON.
        /// </summary>
        /// <param name="utf8json">The stream to persist to.</param>
        /// <param name="options">Optional serializer options.</param>
        public async Task SaveToJsonStream(Stream utf8json, JsonSerializerOptions? options = null)
            => await JsonSerializer.SerializeAsync(utf8json, this, options).ConfigureAwait(false);

        /// <summary>
        /// Persists an index to a stream as JSON.
        /// </summary>
        /// <param name="options">Optional serializer options.</param>
        public string ToJson(JsonSerializerOptions? options = null)
            => JsonSerializer.Serialize(this, options);

        private static Index ProcessDeserializedIndex(
            StemmerBase? stemmer,
            PipelineFunctionRegistry? registry,
            Index index)
        {
            registry ??= new PipelineFunctionRegistry();
            if (!registry.ContainsKey("stemmer"))
            {
                Pipeline.Function stemmerFunction = (stemmer ?? new EnglishStemmer()).StemmerFunction;
                registry.Add("stemmer", stemmerFunction);
            }
            foreach ((string functionName, Pipeline.Function function) in registry)
            {
                index.Pipeline.RegisterFunction(function, functionName);
            }
            return index;
        }
    }
}
