using Castle.MicroKernel;
using System;
using System.Data;

namespace Vertica.Integration.Infrastructure.Database
{
	internal class DbFactory<TConnection> : IDbFactory<TConnection>
	where TConnection : Connection
	{
		private readonly TConnection _connection;

		private readonly IKernel _kernel;

		public DbFactory(TConnection connection, IKernel kernel)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			this._connection = connection;
			this._kernel = kernel;
		}

		public IDbConnection GetConnection()
		{
			return this._connection.GetConnection(this._kernel);
		}

		public IDbSession OpenSession()
		{
			return new DbSession(this.GetConnection());
		}
	}
}