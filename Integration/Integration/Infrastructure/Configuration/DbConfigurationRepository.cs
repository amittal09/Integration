using System;
using System.Data;
using System.Linq;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Configuration
{
	internal class DbConfigurationRepository : IConfigurationRepository
	{
		private readonly Func<IDbFactory> _db;

		public DbConfigurationRepository(Func<IDbFactory> db)
		{
			this._db = db;
		}

		public void Delete(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentException("Value cannot be null or empty.");
			}
			using (IDbSession dbSession = this.OpenSession())
			{
				using (IDbTransaction dbTransaction = dbSession.BeginTransaction(null))
				{
					dbSession.Execute("DELETE FROM Configuration WHERE (Id = @id)", new { id = id });
					dbTransaction.Commit();
				}
			}
		}

		public Vertica.Integration.Infrastructure.Configuration.Configuration Get(string id)
		{
			Vertica.Integration.Infrastructure.Configuration.Configuration configuration;
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentException("Value cannot be null or empty.", "id");
			}
			using (IDbSession dbSession = this.OpenSession())
			{
				configuration = dbSession.Query<Vertica.Integration.Infrastructure.Configuration.Configuration>("SELECT Id, Name, Description, JsonData, Created, Updated, UpdatedBy FROM Configuration WHERE (Id = @id)", new { id = id }).SingleOrDefault<Vertica.Integration.Infrastructure.Configuration.Configuration>();
			}
			return configuration;
		}

		public Vertica.Integration.Infrastructure.Configuration.Configuration[] GetAll()
		{
			Vertica.Integration.Infrastructure.Configuration.Configuration[] array;
			using (IDbSession dbSession = this.OpenSession())
			{
				array = dbSession.Query<Vertica.Integration.Infrastructure.Configuration.Configuration>("SELECT Id, Name, Created, Updated, UpdatedBy FROM Configuration", null).ToArray<Vertica.Integration.Infrastructure.Configuration.Configuration>();
			}
			return array;
		}

		private IDbSession OpenSession()
		{
			return this._db().OpenSession();
		}

		public Vertica.Integration.Infrastructure.Configuration.Configuration Save(Vertica.Integration.Infrastructure.Configuration.Configuration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			configuration.Name = configuration.Name.MaxLength(50);
			configuration.Description = configuration.Description.MaxLength(255);
			configuration.Updated = Time.UtcNow;
			configuration.UpdatedBy = configuration.UpdatedBy.MaxLength(50);
			using (IDbSession dbSession = this.OpenSession())
			{
				using (IDbTransaction dbTransaction = dbSession.BeginTransaction(null))
				{
					dbSession.Execute("\r\nIF NOT EXISTS (SELECT Id FROM Configuration WHERE (Id = @Id))\r\n\tBEGIN\r\n\t\tINSERT INTO Configuration (Id, Name, Description, JsonData, Created, Updated, UpdatedBy)\r\n\t\t\tVALUES (@Id, @Name, @Description, @JsonData, @Updated, @Updated, @UpdatedBy);\r\n\tEND\r\nELSE\r\n\tBEGIN\r\n\t\tUPDATE Configuration SET\r\n\t\t\tJsonData = @JsonData,\r\n\t\t\tUpdated = @Updated,\r\n            UpdatedBy = @UpdatedBy,\r\n            Name = @Name,\r\n\t\t\tDescription = @Description\r\n\t\tWHERE (Id = @Id)\r\n\tEND\r\n", configuration);
					dbTransaction.Commit();
				}
			}
			return configuration;
		}
	}
}