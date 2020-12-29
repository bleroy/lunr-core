using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using LightningDB;
using Lunr;

namespace LunrCoreLmdb
{
    public sealed class LmdbIndex
    {
        public Lazy<LightningEnvironment> Env { get; }

        public string Path { get; }

        public LmdbIndex(string path)
        {
            Env = new Lazy<LightningEnvironment>(() =>
            {
                var config = new EnvironmentConfiguration
                {
                    MaxDatabases = DefaultMaxDatabases,
                    MaxReaders = DefaultMaxReaders,
                    MapSize = DefaultMapSize
                };
                var environment = new LightningEnvironment(path, config);
                environment.Open();
                CreateIfNotExists(environment);
                return environment;
            });
            Path = Env.Value.Path;
        }

		#region Fields 

        public bool AddField(string field, CancellationToken cancellationToken = default)
        {
			cancellationToken.ThrowIfCancellationRequested();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.None);
            using var db = tx.OpenDatabase(configuration: Config);

            tx.Put(db, KeyBuilder.BuildFieldKey(field), Encoding.UTF8.GetBytes(field), PutOptions.NoDuplicateData);
            return tx.Commit() == MDBResultCode.Success;
        }

        public bool RemoveField(string field, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.None);
            using var db = tx.OpenDatabase(configuration: Config);

            tx.Delete(db, KeyBuilder.BuildFieldKey(field));
            return tx.Commit() == MDBResultCode.Success;
        }

        public IEnumerable<string> GetFields(CancellationToken cancellationToken)
        {
            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: Config);
            using var cursor = tx.CreateCursor(db);

            var sr = cursor.SetRange(KeyBuilder.BuildAllFieldsKey());
            if (sr != MDBResultCode.Success)
                yield break;

            var (r, _, v) = cursor.GetCurrent();
			
            while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
            {
                var buffer = v.AsSpan().ToArray();
                yield return Encoding.UTF8.GetString(buffer);
                r = cursor.Next();
            }
        }

		#endregion

		#region Field Vectors

        public bool AddFieldVector(string key, Vector vector, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.None);
            using var db = tx.OpenDatabase(configuration: Config);

            // Key:
            tx.Put(db, KeyBuilder.BuildFieldVectorKeyKey(key), Encoding.UTF8.GetBytes(key), PutOptions.NoDuplicateData);

            // Value:
            tx.Put(db, KeyBuilder.BuildFieldVectorKey(key), vector.Serialize(), PutOptions.NoDuplicateData);

            return tx.Commit() == MDBResultCode.Success;
        }

        public Vector? GetFieldVectorByKey(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: Config);
            using var cursor = tx.CreateCursor(db);

            var sr = cursor.Set(KeyBuilder.BuildFieldVectorKey(key));
            if (sr != MDBResultCode.Success)
                return default;

            var (r, _, v) = cursor.GetCurrent();
            if (r != MDBResultCode.Success)
                return default;

            return v.AsSpan().DeserializeFieldVector();
        }

        public IEnumerable<string> GetFieldVectorKeys(CancellationToken cancellationToken)
        {
            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: Config);
            using var cursor = tx.CreateCursor(db);

            var sr = cursor.SetRange(KeyBuilder.BuildAllFieldVectorKeys());
            if (sr != MDBResultCode.Success)
                yield break;

            var (r, _, v) = cursor.GetCurrent();
			
            while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
            {
                var key = v.AsSpan().ToArray();
                yield return Encoding.UTF8.GetString(key);

                r = cursor.Next();
                if (r != MDBResultCode.Success)
                    yield break;
            }
        }

        public bool RemoveFieldVector(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.None);
            using var db = tx.OpenDatabase(configuration: Config);

            tx.Delete(db, KeyBuilder.BuildFieldVectorKey(key));
            tx.Delete(db, KeyBuilder.BuildFieldVectorKeyKey(key));
            return tx.Commit() == MDBResultCode.Success;
        }

		#endregion

		#region Inverted Index Entries

        public bool AddInvertedIndexEntry(string key, InvertedIndexEntry invertedIndexEntry, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.None);
            using var db = tx.OpenDatabase(configuration: Config);

            tx.Put(db, KeyBuilder.BuildInvertedIndexEntryKey(key), invertedIndexEntry.Serialize(), PutOptions.NoDuplicateData);
            tx.Put(db, KeyBuilder.BuildTokenSetWordKey(key), Encoding.UTF8.GetBytes(key), PutOptions.NoDuplicateData);

            return tx.Commit() == MDBResultCode.Success;
        }

        public InvertedIndexEntry? GetInvertedIndexEntryByKey(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: Config);
            using var cursor = tx.CreateCursor(db);

            var sr = cursor.Set(KeyBuilder.BuildInvertedIndexEntryKey(key));
            if (sr != MDBResultCode.Success)
                return default;

            var (r, _, v) = cursor.GetCurrent();
            return r != MDBResultCode.Success ? default : v.AsSpan().DeserializeInvertedIndexEntry();
        }

        #endregion

        #region TokenSets

        public TokenSet IntersectTokenSets(TokenSet other, CancellationToken cancellationToken)
        {
            // FIXME: This is still reading fully into memory before intersection

            cancellationToken.ThrowIfCancellationRequested();

            var builder = new TokenSet.Builder();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: Config);
            using var cursor = tx.CreateCursor(db);

            var sr = cursor.SetRange(KeyBuilder.BuildAllTokenSetWordKeys());
            if (sr != MDBResultCode.Success)
                return builder.Root;

            var (r, _, v) = cursor.GetCurrent();
			
            while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
            {
                var buffer = v.AsSpan().ToArray();
                var word = Encoding.UTF8.GetString(buffer);
                builder.Insert(word);

                r = cursor.Next();
                if (r != MDBResultCode.Success)
                    break;
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
        
		private static void CreateIfNotExists(LightningEnvironment environment)
		{
			using var tx = environment.BeginTransaction();
			try
			{
				using (tx.OpenDatabase(null, Config))
				{
					tx.Commit();
				}
			}
			catch (LightningException)
			{
				using (tx.OpenDatabase(null, new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create}))
				{
					tx.Commit();
				}
			}
		}

        public ulong GetLength()
		{
			using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.ReadOnly);
			using var db = tx.OpenDatabase(configuration: Config);
			var count = tx.GetEntriesCount(db); // entries also contains handles to databases
			return (ulong) count;
		}

		public void Clear()
		{
			using var tx = Env.Value.BeginTransaction();
			var db = tx.OpenDatabase(configuration: Config);
			tx.TruncateDatabase(db);
			tx.Commit();
		}

		public void Destroy()
		{
			using (var tx = Env.Value.BeginTransaction())
			{
				var db = tx.OpenDatabase(configuration: Config);
				tx.DropDatabase(db);
				tx.Commit();
			}

			if (Env.IsValueCreated)
				Env.Value.Dispose();

			try
			{
				Directory.Delete(Path, true);
			}
			catch (Exception exception)
			{
				Trace.TraceError(exception.ToString());
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

		#region IDisposable
		
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ReSharper disable once EmptyDestructor
        ~LmdbIndex() { }

        public void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (Env.IsValueCreated)
                Env.Value.Dispose();
        }

		#endregion
    }
}