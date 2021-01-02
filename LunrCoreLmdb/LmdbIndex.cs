using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightningDB;
using Lunr;
using Lunr.Multi;

namespace LunrCoreLmdb
{
    public sealed class LmdbIndex : IReadOnlyIndex, IDisposable
    {
        private readonly LightningEnvironment _env;

        public LmdbIndex(string path)
        {
            var config = new EnvironmentConfiguration
            {
                MaxDatabases = DefaultMaxDatabases,
                MaxReaders = DefaultMaxReaders,
                MapSize = DefaultMapSize
            };
            _env = new LightningEnvironment(path, config);
            _env.Open();
            CreateDatabaseIfNotExists(_env);
        }

        /// <summary>
        /// A convenience function for configuring and constructing
        /// a new lunr DelegatedIndex.
        /// 
        /// An `LmdbBuilder` instance is created and the pipeline setup
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
        /// <param name="path">The directory path to the LMDB database used to store this index.</param>
        /// <param name="trimmer">An optional trimmer. Default is a regex-based word trimmer.</param>
        /// <param name="stopWordFilter">An optional stopword filter. Default is English.</param>
        /// <param name="stemmer">An optional stemmer. Default is English.</param>
        /// <param name="config">A Configuration function.</param>
        /// <param name="tokenizer">An optional tokenizer. Default is a ToString() based splitter. </param>
        /// <param name="registry">An optional pipeline function registry. Default filters through the specific trimmer, stopword filter, and stemmer.</param>
        /// <param name="indexingPipeline">An optional indexing pipeline. Default filters through the specific trimmer, stopword filter, and stemmer. </param>
        /// <param name="searchPipeline">An optional search pipeline. Default filters through the stemmer.</param>
        /// <param name="cancellationToken">An optional cancellation token. Default is equivalent to CancellationToken.None.</param>
        /// <param name="fields">The fields for this builder.</param>
        /// <returns>The delegated index.</returns>
        public static async Task<DelegatedIndex> Build(
            string path,
            Func<LmdbBuilder, Task>? config = null!,
            TrimmerBase? trimmer = null!,
            StopWordFilterBase? stopWordFilter = null!,
            StemmerBase? stemmer = null!,
            Tokenizer? tokenizer = null!,
            PipelineFunctionRegistry? registry = null!,
            IEnumerable<string>? indexingPipeline = null!,
            IEnumerable<string>? searchPipeline = null!,
            CancellationToken cancellationToken = default,
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

            var builder = new LmdbBuilder(
                indexingPipeline: idxPipeline,
                searchPipeline: srchPipeline,
                tokenizer: tokenizer ?? new Tokenizer(),
                fields: fields);

            if (config != null)
            {
                await config(builder);
            }

            return builder.Build(path, cancellationToken);
        }

        #region Multi

        public static class Fr
        {
            /// <summary>
            /// A convenience function for configuring and constructing
            /// a new lunr DelegatedIndex.
            /// 
            /// An `LmdbBuilder` instance is created and the pipeline setup
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
            /// <param name="path">The directory path to the LMDB database used to store this index.</param>
            /// <param name="trimmer">An optional trimmer. Default is a regex-based word trimmer.</param>
            /// <param name="stopWordFilter">An optional stopword filter. Default is English.</param>
            /// <param name="stemmer">An optional stemmer. Default is English.</param>
            /// <param name="config">A Configuration function.</param>
            /// <param name="tokenizer">An optional tokenizer. Default is a ToString() based splitter. </param>
            /// <param name="registry">An optional pipeline function registry. Default filters through the specific trimmer, stopword filter, and stemmer.</param>
            /// <param name="indexingPipeline">An optional indexing pipeline. Default filters through the specific trimmer, stopword filter, and stemmer. </param>
            /// <param name="searchPipeline">An optional search pipeline. Default filters through the stemmer.</param>
            /// <param name="cancellationToken">An optional cancellation token. Default is equivalent to CancellationToken.None.</param>
            /// <param name="fields">The fields for this builder.</param>
            /// <returns>The delegated index.</returns>
            public static async Task<DelegatedIndex> Build(
                string path,
                Func<LmdbBuilder, Task>? config = null!,
                Tokenizer? tokenizer = null!,
                PipelineFunctionRegistry? registry = null!,
                IEnumerable<string>? indexingPipeline = null!,
                IEnumerable<string>? searchPipeline = null!,
                CancellationToken cancellationToken = default,
                params Field[] fields)
            {
                Pipeline.Function trimmerFunction = new FrenchTrimmer().FilterFunction;
                Pipeline.Function filterFunction = new FrenchStopWordFilter().FilterFunction;
                Pipeline.Function stemmerFunction = new FrenchStemmer().StemmerFunction;
                registry ??= new PipelineFunctionRegistry();
                registry.Add("trimmer", trimmerFunction);
                registry.Add("stopWordFilter", filterFunction);
                registry.Add("stemmer", stemmerFunction);

                Pipeline idxPipeline = indexingPipeline is null
                    ? new Pipeline(registry, trimmerFunction, filterFunction, stemmerFunction)
                    : new Pipeline(registry, indexingPipeline.Select(function => registry[function]).ToArray());
                Pipeline srchPipeline = searchPipeline is null
                    ? new Pipeline(registry, stemmerFunction)
                    : new Pipeline(registry, searchPipeline.Select(function => registry[function]).ToArray());

                var builder = new LmdbBuilder(
                    indexingPipeline: idxPipeline,
                    searchPipeline: srchPipeline,
                    tokenizer: tokenizer ?? new Tokenizer(),
                    fields: fields);

                if (config != null)
                {
                    await config(builder);
                }

                return builder.Build(path, cancellationToken);
            }
        }

        public static class It
        {
            public static async Task<DelegatedIndex> Build(
                string path,
                Func<LmdbBuilder, Task>? config = null!,
                Tokenizer? tokenizer = null!,
                PipelineFunctionRegistry? registry = null!,
                IEnumerable<string>? indexingPipeline = null!,
                IEnumerable<string>? searchPipeline = null!,
                CancellationToken cancellationToken = default,
                params Field[] fields)
            {
                Pipeline.Function trimmerFunction = new ItalianTrimmer().FilterFunction;
                Pipeline.Function filterFunction = new ItalianStopWordFilter().FilterFunction;
                Pipeline.Function stemmerFunction = new ItalianStemmer().StemmerFunction;

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

                var builder = new LmdbBuilder(
                    indexingPipeline: idxPipeline,
                    searchPipeline: srchPipeline,
                    tokenizer: tokenizer ?? new Tokenizer(),
                    fields: fields);

                if (config != null)
                {
                    await config(builder);
                }

                return builder.Build(path, cancellationToken);
            }
        }

        #endregion

        #region Fields 

        public bool AddField(string field, CancellationToken cancellationToken = default) => WithWritableTransaction((db, tx) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            tx.Put(db, KeyBuilder.BuildFieldKey(field), Encoding.UTF8.GetBytes(field), PutOptions.NoDuplicateData);
            return tx.Commit() == MDBResultCode.Success;
        });

        public bool RemoveField(string field, CancellationToken cancellationToken = default) => WithWritableTransaction((db, tx) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            tx.Delete(db, KeyBuilder.BuildFieldKey(field));
            return tx.Commit() == MDBResultCode.Success;
        });

        public IEnumerable<string> GetFields(CancellationToken cancellationToken) => WithReadOnlyCursor(c => GetFields(c, cancellationToken));

        private static IList<string> GetFields(LightningCursor cursor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fields = new List<string>();

            var allFieldsKey = KeyBuilder.GetAllFieldsKey();
            var sr = cursor.SetRange(allFieldsKey);
            if (sr != MDBResultCode.Success)
                return fields;

            var (r, k, v) = cursor.GetCurrent();

            while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
            {
                if (!k.AsSpan().StartsWith(allFieldsKey))
                    break;

                var buffer = v.AsSpan().ToArray();
                var field = Encoding.UTF8.GetString(buffer);
                fields.Add(field);

                r = cursor.Next();
                if (r == MDBResultCode.Success)
                    (r, k, v) = cursor.GetCurrent();
            }

            return fields;
        }

        #endregion

        #region Field Vectors

        public bool AddFieldVector(string key, Vector vector, CancellationToken cancellationToken = default)
        {
            return WithWritableTransaction((db, tx) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                tx.Put(db, KeyBuilder.BuildFieldVectorKeyKey(key), Encoding.UTF8.GetBytes(key), PutOptions.NoDuplicateData);
                tx.Put(db, KeyBuilder.BuildFieldVectorValueKey(key), vector.Serialize(), PutOptions.NoDuplicateData);
                return tx.Commit() == MDBResultCode.Success;
            });
        }

        public Vector? GetFieldVectorByKey(string key, CancellationToken cancellationToken) => WithReadOnlyCursor(cursor =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (r, k, v) = cursor.SetKey(KeyBuilder.BuildFieldVectorValueKey(key));
            if (r != MDBResultCode.Success)
                return default;

            return v.AsSpan().DeserializeFieldVector();
        });

        public IEnumerable<string> GetFieldVectorKeys(CancellationToken cancellationToken) => WithReadOnlyCursor(c => GetFieldVectorKeys(c, cancellationToken));

        private static IList<string> GetFieldVectorKeys(LightningCursor cursor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var keys = new List<string>();

            var allFieldVectorKeys = KeyBuilder.GetAllFieldVectorKeys();
            var sr = cursor.SetRange(allFieldVectorKeys);
            if (sr != MDBResultCode.Success)
                return keys;

            var (r, k, v) = cursor.GetCurrent();
            
            while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
            {
                if (!k.AsSpan().StartsWith(allFieldVectorKeys))
                    break;

                var buffer = v.AsSpan().ToArray();
                var key = Encoding.UTF8.GetString(buffer);
                keys.Add(key);

                r = cursor.Next();
                if(r == MDBResultCode.Success)
                    (r, k, v) = cursor.GetCurrent();
            }

            return keys;
        }

        public bool RemoveFieldVector(string key, CancellationToken cancellationToken)
        {
            return WithWritableTransaction((db, tx) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                tx.Delete(db, KeyBuilder.BuildFieldVectorValueKey(key));
                tx.Delete(db, KeyBuilder.BuildFieldVectorKeyKey(key));
                return tx.Commit() == MDBResultCode.Success;
            });
        }

        #endregion

        #region Inverted Index Entries

        public bool AddInvertedIndexEntry(string key, InvertedIndexEntry invertedIndexEntry, CancellationToken cancellationToken = default)
        {
            return WithWritableTransaction((db, tx) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                tx.Put(db, KeyBuilder.BuildInvertedIndexEntryKey(key), invertedIndexEntry.Serialize(), PutOptions.NoDuplicateData);
                tx.Put(db, KeyBuilder.BuildTokenSetWordKey(key), Encoding.UTF8.GetBytes(key), PutOptions.NoDuplicateData);
                return tx.Commit() == MDBResultCode.Success;
            });
        }

        public InvertedIndexEntry? GetInvertedIndexEntryByKey(string key, CancellationToken cancellationToken) => WithReadOnlyCursor(c => GetInvertedIndexEntryByKey(key, c, cancellationToken));

        private static InvertedIndexEntry? GetInvertedIndexEntryByKey(string key, LightningCursor c, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (r, k, v) = c.SetKey(KeyBuilder.BuildInvertedIndexEntryKey(key));
            return r != MDBResultCode.Success ? default : v.AsSpan().DeserializeInvertedIndexEntry();
        }

        #endregion

        #region TokenSets

        public TokenSet IntersectTokenSets(TokenSet other, CancellationToken cancellationToken) => WithReadOnlyCursor(c => IntersectTokenSets(other, c, cancellationToken));

        private static TokenSet IntersectTokenSets(TokenSet other, LightningCursor cursor, CancellationToken cancellationToken)
        {
            // FIXME: This is still reading fully into memory before intersection

            cancellationToken.ThrowIfCancellationRequested();

            var builder = new TokenSet.Builder();

            var allWordKeys = KeyBuilder.BuildAllTokenSetWordKeys();
            var sr = cursor.SetRange(allWordKeys);
            if (sr != MDBResultCode.Success)
                return builder.Root;

            var (r, k, v) = cursor.GetCurrent();

            while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
            {
                if (!k.AsSpan().StartsWith(allWordKeys))
                    break;

                var buffer = v.AsSpan().ToArray();
                var word = Encoding.UTF8.GetString(buffer);
                builder.Insert(word);

                r = cursor.Next();
                if (r == MDBResultCode.Success)
                    (r, k, v) = cursor.GetCurrent();
            }

            return builder.Root.Intersect(other);
        }

        #endregion

        #region Management

        private const ushort MaxKeySizeBytes = 511;

        private const int DefaultMaxReaders = 126;
        private const int DefaultMaxDatabases = 5;
        private const long DefaultMapSize = 10_485_760;

        private static readonly DatabaseConfiguration Config = new DatabaseConfiguration {Flags = DatabaseOpenFlags.None};
        
        private static void CreateDatabaseIfNotExists(LightningEnvironment environment)
        {
            using var tx = environment.BeginTransaction();
            try
            {
                using var db = tx.OpenDatabase(null, Config);
                tx.Commit();
            }
            catch (LightningException)
            {
                using (tx.OpenDatabase(null, new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create}))
                {
                    tx.Commit();
                }
            }
        }

        private T WithReadOnlyCursor<T>(Func<LightningCursor, T> func)
        {
            using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: Config);
            using var cursor = tx.CreateCursor(db);
            var result = func.Invoke(cursor);
            return result;
        }

        private T WithWritableTransaction<T>(Func<LightningDatabase, LightningTransaction, T> func)
        {
            using var tx = _env.BeginTransaction(TransactionBeginFlags.None);
            using var db = tx.OpenDatabase(configuration: Config);
            var result = func.Invoke(db, tx);
            return result;
        }

        public void Dispose()
        {
            _env.Dispose();
        }

        #endregion
        
        #region Delegates

        public IEnumerable<string> GetFields() => GetFields(CancellationToken.None);

        public TokenSet IntersectTokenSets(TokenSet other) => IntersectTokenSets(other, CancellationToken.None);

        public Vector? GetFieldVectorByKey(string key) => GetFieldVectorByKey(key, CancellationToken.None);

        public IEnumerable<string> GetFieldVectorKeys() => GetFieldVectorKeys(CancellationToken.None);

        public InvertedIndexEntry? GetInvertedIndexEntryByKey(string key) => GetInvertedIndexEntryByKey(key, CancellationToken.None);

        #endregion
    }
}