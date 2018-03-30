using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Parsing;

namespace Vertica.Integration.Infrastructure.Database.Extensions
{
	public static class DbExtensions
	{
		public static string QueryToCsv(this IDbSession session, string sql, dynamic param = null, Action<CsvRow.ICsvRowBuilderConfiguration> configuration = null)
		{
            return string.Empty;
			//if (session == null)
			//{
			//	throw new ArgumentNullException("session");
			//}
			//if (string.IsNullOrWhiteSpace(sql))
			//{
			//	throw new ArgumentException("Value cannot be null or empty.", "sql");
			//}
			//IEnumerable<object> objs = (IEnumerable<object>)session.Query(sql, param);
			//string[] array = null;
			//return CsvRow.BeginRows(new string[0]).Configure((CsvRow.ICsvRowBuilderConfiguration x) => {
			//	if (configuration == null)
			//	{
			//		x.ReturnHeaderAsRow();
			//		return;
			//	}
			//	configuration(x);
			//}).From<object>(objs, (object data) => {
			//	if (DbExtensions.Wrap== null)
			//	{
			//		DbExtensions.<>o__1.<>p__2 = CallSite<Func<CallSite, object, IDictionary<string, object>>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(IDictionary<string, object>), typeof(DbExtensions)));
			//	}
			//	IDictionary<string, object> target = DbExtensions.<>o__1.<>p__2.Target(DbExtensions.<>o__1.<>p__2, data);
			//	if (array == null)
			//	{
			//		array = target.Keys.ToArray<string>();
			//	}
			//	ICollection<object> values = target.Values;
			//	Func<object, string> u003cu003e9_12 = DbExtensions.<>c.<>9__1_2;
			//	if (u003cu003e9_12 == null)
			//	{
			//		u003cu003e9_12 = (object x) => {
			//			if (x == null)
			//			{
			//				return null;
			//			}
			//			return x.ToString();
			//		};
			//		DbExtensions.<>c.<>9__1_2 = u003cu003e9_12;
			//	}
			//	return values.Select<object, string>(u003cu003e9_12).ToArray<string>();
			//}).Headers(array).ToString();
		}

		internal static T Wrap<T>(this IDbSession session, Func<IDbSession, T> action)
		{
			T t;
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			try
			{
				t = action(session);
			}
			catch (SqlException sqlException)
			{
				throw new IntegrationDbException(sqlException);
			}
			return t;
		}
	}
}