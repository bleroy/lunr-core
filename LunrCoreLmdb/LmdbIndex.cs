using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LightningDB;
using Lunr;

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

            var vector = v.AsSpan().DeserializeFieldVector();
            return vector;
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

            if (r != MDBResultCode.Success)
                return default;

            var invertedIndexEntry = v.AsSpan().DeserializeInvertedIndexEntry();
            return invertedIndexEntry;
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

        #endregion
        
        #region Delegates

        public IEnumerable<string> GetFields() => GetFields(CancellationToken.None);

        public TokenSet IntersectTokenSets(TokenSet other) => IntersectTokenSets(other, CancellationToken.None);

        public Vector? GetFieldVectorByKey(string key) => GetFieldVectorByKey(key, CancellationToken.None);

        public IEnumerable<string> GetFieldVectorKeys() => GetFieldVectorKeys(CancellationToken.None);

        public InvertedIndexEntry? GetInvertedIndexEntryByKey(string key) => GetInvertedIndexEntryByKey(key, CancellationToken.None);

        #endregion

        
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
    }
}