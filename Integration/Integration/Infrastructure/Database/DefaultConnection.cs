using Castle.MicroKernel;
using System;
using System.Data;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Infrastructure.Database
{
	public sealed class DefaultConnection : Connection
	{
		private readonly bool _isDisabled;

		private readonly Connection _connection;

		public static DefaultConnection Disabled
		{
			get
			{
				return new DefaultConnection();
			}
		}

		internal bool IsDisabled
		{
			get
			{
				return this._isDisabled;
			}
		}

		private DefaultConnection() : base(Vertica.Integration.Infrastructure.ConnectionString.FromText(string.Empty))
		{
			this._isDisabled = true;
		}

		internal DefaultConnection(Vertica.Integration.Infrastructure.ConnectionString connectionString) : base(connectionString)
		{
		}

		internal DefaultConnection(Connection connection) : base(connection.ConnectionString)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			this._connection = connection;
		}

		protected internal override IDbConnection GetConnection(IKernel kernel)
		{
			if (this._connection == null)
			{
				return base.GetConnection(kernel);
			}
			return this._connection.GetConnection(kernel);
		}
	}
}