using System;
using System.Data;
using System.Data.SqlClient;

namespace Vertica.Integration.Infrastructure.Database
{
	internal class DbFactory : IDbFactory, IDbFactory<DefaultConnection>
	{
		private readonly IDbFactory<DefaultConnection> _decoree;

		public DbFactory(IDbFactory<DefaultConnection> decoree)
		{
			if (decoree == null)
			{
				throw new ArgumentNullException("decoree");
			}
			this._decoree = decoree;
		}

		public IDbConnection GetConnection()
		{
			return this._decoree.GetConnection();
		}

		public IDbSession OpenSession()
		{
			IDbSession dbSession;
			try
			{
				dbSession = this._decoree.OpenSession();
			}
			catch (SqlException sqlException)
			{
				throw new IntegrationDbException(sqlException);
			}
			return dbSession;
		}
	}
}