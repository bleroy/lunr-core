using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lunr
{
    /// <summary>
    /// `Builder` performs indexing on a set of documents and
    /// returns instances of `Index` ready for querying.
    /// 
    /// All configuration of the index is done via the builder, the
    /// fields to index, the document reference, the text processing
    /// pipeline and document scoring parameters are all set on the
    /// builder before indexing.
    /// </summary>
    public class Builder
    {
        private readonly IDictionary<string, Field> _fields = new Dictionary<string, Field>();
        private readonly IDictionary<string, Document> _documents = new Dictionary<string, Document>();
        private readonly IDictionary<FieldReference, IDictionary<Token, int>> _fieldTermFrequencies
            = new Dictionary<FieldReference, IDictionary<Token, int>>();
        private readonly IDictionary<FieldReference, int> _fieldLengths
            = new Dictionary<FieldReference, int>();
        private IDictionary<string, int> _averageFieldLength
            = new Dictionary<string, int>();
        private IDictionary<string, Vector> _fieldVectors = new Dictionary<string, Vector>();
        private readonly ITokenizer _tokenizer;
        private readonly IPipeline _indexingPipeline;
        private readonly IPipeline _searchPipeline;
        private int _termIndex = 0;
        private double _b = 0.75;

        /// <summary>
        /// Creates a builder with a list of fields.
        /// </summary>
        /// <param name="tokenizer">Function for splitting strings into tokens for indexing.</param>
        /// <param name="indexingPipeline">The pipeline performs text processing on tokens before indexing.</param>
        /// <param name="searchPipeline">A pipeline for processing search terms before querying the index.</param>
        /// <param name="fields">The fields for this builder.</param>
        public Builder(
            ITokenizer tokenizer,
            IPipeline indexingPipeline,
            IPipeline searchPipeline,
            params Field[] fields) : this(fields)
        {
            InitializeFields(fields);

            _tokenizer = tokenizer;
            _indexingPipeline = indexingPipeline;
            _searchPipeline = searchPipeline;
        }

        /// <summary>
        /// Creates a builder with a list of fields.
        /// </summary>
        /// <param name="fields">The fields for this builder.</param>
        public Builder(params Field[] fields)
        {
            InitializeFields(fields);

            _tokenizer = new Tokenizer();
            _indexingPipeline = new Pipeline();
            _searchPipeline = new Pipeline();
        }

        /// <summary>
        /// The document field used as the document reference. Every document must have this field.
        /// 
        /// The type of this field in the document should be a string, if it is not a string it will be
        /// coerced into a string by calling ToString.
        /// 
        /// The default reference field is 'id'.
        /// 
        /// The reference field should _not_ be changed during indexing, it should be set before any documents are
        /// added to the index. Changing it during indexing can lead to inconsistent results.
        /// </summary>
        public string ReferenceField { get; set; } = "id";

        /// <summary>
        /// The list of fields for this builder.
        /// </summary>
        public IEnumerable<Field> Fields => _fields.Values;

        /// <summary>
        /// The inverted index.
        /// </summary>
        public InvertedIndex InvertedIndex { get; } = new InvertedIndex();

        /// <summary>
        /// A list of metadata keys that have been whitelisted for entry in the index.
        /// </summary>
        public IList<string> MetadataWhiteList { get; } = new List<string>();

        /// <summary>
        /// The total number of documents indexed.
        /// </summary>
        public int DocumentCount { get; private set; } = 0;

        /// <summary>
        /// A parameter to tune the amount of field length normalisation that is applied when calculating relevance scores.
        /// A value of 0 will completely disable any normalisation and a value of 1 will fully normalise field lengths.
        /// The default is 0.75. Values of b will be clamped to the range 0 - 1.
        /// </summary>
        public double FieldLengthNormalizationFactor
        {
            get => _b;
            set => _b = value < 0 ? 0 : value > 1 ? 1 : value;
        }

        /// <summary>
        /// A parameter that controls the speed at which a rise in term frequency results in term frequency saturation.
        /// The default value is 1.2.
        /// Setting this to a higher value will give slower saturation levels, a lower value will result in quicker saturation.
        /// </summary>
        public double TermFrequencySaturationFactor { get; set; } = 1.2;

        /// <summary>
        /// Adds a field to the list of document fields that will be indexed. Every document being
        /// indexed should have this field.Null values for this field in indexed documents will
        /// not cause errors but will limit the chance of that document being retrieved by searches.
        ///
        /// All fields should be added before adding documents to the index.Adding fields after
        /// a document has been indexed will have no effect on already indexed documents.
        ///
        /// Fields can be boosted at build time.This allows terms within that field to have more
        /// importance when ranking search results. Use a field boost to specify that matches within
        /// one field are more important than other fields.
        /// </summary>
        /// <param name="field">A field to index in all documents.</param>
        public void AddField(Field field)
        {
            if (field.Name.IndexOf('/') != -1) throw new ArgumentOutOfRangeException($"Field '{field.Name}' contains illegal character '/'");

            _fields.Add(field.Name, field);
        }


        /// <summary>
        /// Adds a field to the list of document fields that will be indexed. Every document being
        /// indexed should have this field.Null values for this field in indexed documents will
        /// not cause errors but will limit the chance of that document being retrieved by searches.
        ///
        /// All fields should be added before adding documents to the index.Adding fields after
        /// a document has been indexed will have no effect on already indexed documents.
        ///
        /// Fields can be boosted at build time.This allows terms within that field to have more
        /// importance when ranking search results. Use a field boost to specify that matches within
        /// one field are more important than other fields.
        /// </summary>
        /// <param name="fieldName">The name of a field to index in all documents.</param>
        public void AddField(string fieldName)
            => AddField(new Field<string>(fieldName));

        /// <summary>
        /// Adds a document to the index.
        ///
        /// Before adding fields to the index the index should have been fully setup, with the document
        /// ref and all fields to index already having been specified.
        ///
        /// The document must have a field name as specified by the ref (by default this is 'id') and
        /// it should have all fields defined for indexing, though null or undefined values will not
        /// cause errors.
        ///
        /// Entire documents can be boosted at build time. Applying a boost to a document indicates that
        /// this document should rank higher in search results than other documents.
        /// </summary>
        /// <param name="doc">The document to index.</param>
        /// <param name="attributes">An optional set of attributes associated with this document.</param>
        public async Task Add(
            Document doc,
            IDictionary<string, object> attributes,
            CultureInfo culture,
            CancellationToken cancellationToken)
        {
            string docRef = doc[ReferenceField].ToString();

            _documents[docRef] = new Document(attributes);
            DocumentCount++;

            foreach (Field field in Fields)
            {
                object fieldValue = field.ExtractValue(doc);
                var metadata = new Dictionary<string, object>
                {
                    { "fieldName", field.Name }
                };
                IEnumerable<Token> tokens = _tokenizer.Tokenize(
                    fieldValue,
                    metadata,
                    culture ?? CultureInfo.CurrentCulture);
                IAsyncEnumerable<Token> terms = _indexingPipeline.Run(
                    tokens.ToAsyncEnumerable(cancellationToken), cancellationToken);
                var fieldReference = new FieldReference(docRef, field.Name);
                var fieldTerms = new Dictionary<Token, int>();

                _fieldTermFrequencies[fieldReference] = fieldTerms;
                _fieldLengths[fieldReference] = 0;

                int termsCount = 0;
                await foreach (Token term in terms)
                {
                    termsCount++;
                    if (!fieldTerms.ContainsKey(term)) fieldTerms.Add(term, 0);
                    fieldTerms[term]++;

                    // add to inverted index
                    // create an initial posting if one doesn't exist
                    if (!InvertedIndex.ContainsKey(term))
                    {
                        var posting = new Posting
                        {
                            Index = _termIndex++
                        };
                        foreach (Field postingField in Fields)
                        {
                            posting.Add(
                                postingField.Name,
                                new Dictionary<string, IDictionary<string, IList<object>>>());
                        }
                        InvertedIndex[term] = posting;

                        // Add an entry for this term/fieldName/docRef to the invertedIndex.
                        if (!InvertedIndex[term][field.Name].ContainsKey(docRef))
                        {
                            InvertedIndex[term][field.Name].Add(
                                docRef,
                                new Dictionary<string, IList<object>>());
                        }

                        // store all whitelisted metadata about this token in the inverted index.
                        foreach (string metadataKey in MetadataWhiteList)
                        {
                            object termMetadata = term.Metadata[metadataKey];

                            if (!InvertedIndex[term][field.Name][docRef].ContainsKey(metadataKey))
                            {
                                InvertedIndex[term][field.Name][docRef].Add(metadataKey, new List<object>());
                            }

                            InvertedIndex[term][field.Name][docRef][metadataKey].Add(termMetadata);
                        }
                    }
                    // store the length of this field for this document
                    _fieldLengths[fieldReference] = termsCount;
                }
            }
        }

        /// <summary>
        /// Adds a document to the index.
        ///
        /// Before adding fields to the index the index should have been fully setup, with the document
        /// ref and all fields to index already having been specified.
        ///
        /// The document must have a field name as specified by the ref (by default this is 'id') and
        /// it should have all fields defined for indexing, though null or undefined values will not
        /// cause errors.
        ///
        /// Entire documents can be boosted at build time. Applying a boost to a document indicates that
        /// this document should rank higher in search results than other documents.
        /// </summary>
        /// <param name="doc">The document to index.</param>
        public async Task Add(
            Document doc,
            CultureInfo culture,
            CancellationToken cancellationToken)
            => await Add(doc, new Dictionary<string, object>(), culture, cancellationToken);

        /// <summary>
        /// Adds a document to the index.
        ///
        /// Before adding fields to the index the index should have been fully setup, with the document
        /// ref and all fields to index already having been specified.
        ///
        /// The document must have a field name as specified by the ref (by default this is 'id') and
        /// it should have all fields defined for indexing, though null or undefined values will not
        /// cause errors.
        ///
        /// Entire documents can be boosted at build time. Applying a boost to a document indicates that
        /// this document should rank higher in search results than other documents.
        /// </summary>
        /// <param name="doc">The document to index.</param>
        public async Task Add(
            Document doc,
            CancellationToken cancellationToken)
            => await Add(doc, new Dictionary<string, object>(), CultureInfo.CurrentCulture, cancellationToken);

        /// <summary>
        /// Adds a document to the index.
        ///
        /// Before adding fields to the index the index should have been fully setup, with the document
        /// ref and all fields to index already having been specified.
        ///
        /// The document must have a field name as specified by the ref (by default this is 'id') and
        /// it should have all fields defined for indexing, though null or undefined values will not
        /// cause errors.
        ///
        /// Entire documents can be boosted at build time. Applying a boost to a document indicates that
        /// this document should rank higher in search results than other documents.
        /// </summary>
        /// <param name="doc">The document to index.</param>
        /// <param name="attributes">An optional set of attributes associated with this document.</param>
        public async Task Add(
            Document doc,
            IDictionary<string, object> attributes,
            CancellationToken cancellationToken)
            => await Add(doc, attributes, CultureInfo.CurrentCulture, cancellationToken);

        /// <summary>
        /// Builds the index, creating an instance of lunr.Index.
        ///
        /// This completes the indexing process and should only be called
        /// once all documents have been added to the index.
        /// </summary>
        /// <returns>The index.</returns>
        public Index Build()
        {
            CalculateAverageFieldLengths();
            CreateFieldVectors();

            return new Index(
                InvertedIndex,
                _fieldVectors,
                CreateTokenSet(),
                Fields,
                _searchPipeline);
        }

        /// <summary>
        /// Calculates the average document length for this index.
        /// </summary>
        private void CalculateAverageFieldLengths()
        {
            var documentsWithField = new Dictionary<string, int>();
            var accumulator = new Dictionary<string, int>();

            foreach (FieldReference fieldRef in _fieldLengths.Keys)
            {
                string fieldName = fieldRef.FieldName;

                if (!documentsWithField.ContainsKey(fieldName)) documentsWithField.Add(fieldName, 0);
                documentsWithField[fieldName]++;

                if (!accumulator.ContainsKey(fieldName)) accumulator.Add(fieldName, 0);
                accumulator[fieldName] += _fieldLengths[fieldRef];
            }

            foreach (Field field in Fields)
            {
                accumulator[field.Name] /= documentsWithField[field.Name];
            }

            _averageFieldLength = accumulator;
        }

        /// <summary>
        /// Builds a vector space model of every document using Vector.
        /// </summary>
        private void CreateFieldVectors()
        {
            var termIdfCache = new Dictionary<Token, double>();
            var fieldVectors = new Dictionary<string, Vector>();

            foreach (FieldReference fieldRef in _fieldLengths.Keys)
            {
                var fieldVector = new Vector();
                IDictionary<Token, int> termFrequencies = _fieldTermFrequencies[fieldRef];
                double fieldBoost = _fields.TryGetValue(fieldRef.FieldName, out Field field)
                    ? field.Boost : 1;
                double docBoost = _documents.TryGetValue(fieldRef.DocumentReference, out Document doc)
                    ? doc.Boost : 1;

                foreach ((Token term, int tf) in termFrequencies)
                {
                    int termIndex = InvertedIndex[term].Index;

                    if (!termIdfCache.TryGetValue(term, out double idf))
                    {
                        idf = Util.InverseDocumentFrequency(InvertedIndex[term], _documents.Count);
                        termIdfCache.Add(term, idf);
                    }

                    double score = idf * (TermFrequencySaturationFactor + 1) * tf
                        / (TermFrequencySaturationFactor * (1 - _b + _b * (_fieldLengths[fieldRef] / _averageFieldLength[field.Name])) + tf)
                        * fieldBoost
                        * docBoost;
                    double scoreWithPrecision = Math.Round(score * 1000) / 1000;
                    // Converts 1.23456789 to 1.234.
                    // Reducing the precision so that the vectors take up less
                    // space when serialised. Doing it now so that they behave
                    // the same before and after serialisation.

                    fieldVector.Insert(termIndex, scoreWithPrecision);
                }

                fieldVectors.Add(fieldRef.FieldName, fieldVector);
            }

            _fieldVectors = fieldVectors;
        }

        /// <summary>
        /// Creates a token set of all tokens in the index using TokenSet.
        /// </summary>
        private TokenSet CreateTokenSet()
            =>TokenSet.FromArray(InvertedIndex
                .Keys
                .OrderBy(k => k));

        private void InitializeFields(params Field[] fields)
        {
            if (fields.Select(f => f.Name).Distinct().Count() < fields.Length)
                throw new InvalidOperationException("Builder definition contains duplicate fields.");

            foreach (Field field in fields)
            {
                AddField(field);
            }
        }
    }
}
