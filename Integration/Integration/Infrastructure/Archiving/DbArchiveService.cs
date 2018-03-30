using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Extensions;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Archiving
{
	internal class DbArchiveService : IArchiveService
	{
		private readonly Func<IDbFactory> _db;

		public DbArchiveService(Func<IDbFactory> db)
		{
			this._db = db;
		}

		public BeginArchive Create(string name, Action<ArchiveCreated> onCreated = null)
		{
			return new BeginArchive(name, (MemoryStream stream, ArchiveOptions options) => {
				int num;
				using (IDbSession dbSession = this.OpenSession())
				{
					using (IDbTransaction dbTransaction = dbSession.BeginTransaction(null))
					{
						byte[] array = stream.ToArray();
						num = dbSession.Wrap<int>((IDbSession s) => s.ExecuteScalar<int>("INSERT INTO Archive (Name, BinaryData, ByteSize, Created, Expires, GroupName) VALUES (@name, @binaryData, @byteSize, @created, @expires, @groupName);SELECT CAST(SCOPE_IDENTITY() AS INT);", new { name = options.Name.MaxLength(255), binaryData = array, byteSize = (int)array.Length, created = Time.UtcNow, expires = options.Expires, groupName = options.GroupName }));
						dbTransaction.Commit();
					}
				}
				if (onCreated != null)
				{
					onCreated(new ArchiveCreated(num.ToString(), new string[0]));
				}
			});
		}

		public int Delete(DateTimeOffset olderThan)
		{
			int num;
			using (IDbSession dbSession = this.OpenSession())
			{
				using (IDbTransaction dbTransaction = dbSession.BeginTransaction(null))
				{
					int num1 = dbSession.Wrap<int>((IDbSession s) => s.Execute("DELETE FROM Archive WHERE Created <= @olderThan", new { olderThan = olderThan }));
					dbTransaction.Commit();
					num = num1;
				}
			}
			return num;
		}

		public int DeleteExpired()
		{
			int num;
			using (IDbSession dbSession = this.OpenSession())
			{
				using (IDbTransaction dbTransaction = dbSession.BeginTransaction(null))
				{
					int num1 = dbSession.Wrap<int>((IDbSession s) => s.Execute("DELETE FROM Archive WHERE Expires <= @now", new { now = Time.UtcNow }));
					dbTransaction.Commit();
					num = num1;
				}
			}
			return num;
		}

		public byte[] Get(string id)
		{
			byte[] numArray;
			int num;
			if (!int.TryParse(id, out num))
			{
				return null;
			}
			using (IDbSession dbSession = this.OpenSession())
			{
				numArray = dbSession.Wrap<IEnumerable<byte[]>>((IDbSession s) => s.Query<byte[]>("SELECT BinaryData FROM Archive WHERE Id = @Id", new { Id = num })).SingleOrDefault<byte[]>();
			}
			return numArray;
		}

		public Archive[] GetAll()
		{
			Archive[] array;
			using (IDbSession dbSession = this.OpenSession())
			{
				array = dbSession.Wrap<IEnumerable<Archive>>((IDbSession s) => s.Query<Archive>("SELECT Id, Name, ByteSize, Created, GroupName, Expires FROM Archive", null)).ToArray<Archive>();
			}
			return array;
		}

		private IDbSession OpenSession()
		{
			return this._db().OpenSession();
		}
	}
}