using System;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Database.Extensions
{
	public static class SqlDbExtensions
	{
		private readonly static Range<DateTime> SqlDateTimeRange;

		static SqlDbExtensions()
		{
			SqlDbExtensions.SqlDateTimeRange = new Range<DateTime>(SqlDateTime.MinValue.Value, SqlDateTime.MaxValue.Value);
		}

		public static DateTime NormalizeToSqlDateTime(this DateTime dateTime)
		{
			return SqlDbExtensions.SqlDateTimeRange.Limit(dateTime);
		}
	}
}