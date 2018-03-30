using Castle.MicroKernel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Infrastructure.Database
{
	public abstract class Connection
	{
		public Vertica.Integration.Infrastructure.ConnectionString ConnectionString
		{
			get;
		}

		protected Connection(Vertica.Integration.Infrastructure.ConnectionString connectionString)
		{
			if (connectionString == null)
			{
				throw new ArgumentNullException("connectionString");
			}
			this.ConnectionString = connectionString;
		}

		protected internal virtual IDbConnection GetConnection(IKernel kernel)
		{
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			return new SqlConnection(this.ConnectionString);
		}
	}
}