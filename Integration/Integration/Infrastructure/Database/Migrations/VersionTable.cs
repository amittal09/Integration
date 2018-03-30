using FluentMigrator.VersionTableInfo;
using System;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	[VersionTableMetaData]
	public class VersionTable : DefaultVersionTableMetaData
	{
		public override string TableName
		{
			get
			{
				return string.Concat("BuiltIn_", base.TableName);
			}
		}

		public VersionTable()
		{
		}
	}
}