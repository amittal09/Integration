using System;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace Vertica.Integration.Infrastructure.Database
{
	[Serializable]
	public class IntegrationDbException : Exception
	{
		public IntegrationDbException()
		{
		}

		public IntegrationDbException(SqlException inner) : base(string.Format("{0}\r\n\r\nMake sure the database is created, that it is functional and up-to-date with the latest migrations. \r\n\r\nTry run the \"MigrateTask\" to ensure it's running with the latest database schema.\r\n\r\nIf this error continues, check your connection string and permissions to the database.", inner.Message), inner)
		{
		}

		protected IntegrationDbException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}