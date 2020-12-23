using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using LightningDB;
using Lunr;

namespace LunrCore.Lmdb
{
    public sealed class LmdbIndex
    {
        public LmdbIndex(string path)
        {
            Init(path);
        }

        public bool AddField(string field, CancellationToken cancellationToken = default)
        {
			cancellationToken.ThrowIfCancellationRequested();

            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.None);
            using var db = tx.OpenDatabase(configuration: Config);

            tx.Put(db, FieldKeyBuilder.BuildFieldKey(field), Encoding.UTF8.GetBytes(field), PutOptions.NoDuplicateData);
            return tx.Commit() == MDBResultCode.Success;
        }

        public IEnumerable<string> GetFields(CancellationToken cancellationToken)
        {
            using var tx = Env.Value.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: Config);
            using var cursor = tx.CreateCursor(db);

            var sr = cursor.SetRange(FieldKeyBuilder.BuildAllFieldsKey());
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

		#region Management

        private const ushort MaxKeySizeBytes = 511;

        private const int DefaultMaxReaders = 126;
        private const int DefaultMaxDatabases = 5;
        private const long DefaultMapSize = 10_485_760;

        private static readonly DatabaseConfiguration Config = new DatabaseConfiguration {Flags = DatabaseOpenFlags.None};

        public Lazy<LightningEnvironment> Env;

        public string Path { get; private set; }

		public void Init(string path)
		{
			if (Env != default && Env.IsValueCreated)
				return;
			Env ??= new Lazy<LightningEnvironment>(() =>
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

			if (Env != null && Env.IsValueCreated)
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

        public TokenSet IntersectTokenSets(TokenSet other)
        {
            throw new NotImplementedException();
        }

        public Vector GetFieldVectorByKey(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFieldVectorKeys()
        {
            throw new NotImplementedException();
        }

        public InvertedIndexEntry GetInvertedIndexByKey(string key)
        {
            throw new NotImplementedException();
        }

        #endregion

		#region IDisposable
		
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LmdbIndex() { }

        public void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (Env != null && Env.IsValueCreated)
                Env.Value.Dispose();
        }

		#endregion
    }
}