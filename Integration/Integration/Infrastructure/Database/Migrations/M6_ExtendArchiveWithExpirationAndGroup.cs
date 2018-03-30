using FluentMigrator;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Execute;
using System;
using Vertica.Integration.Domain.Core;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	[Migration(6L)]
	public class M6_ExtendArchiveWithExpirationAndGroup : IntegrationMigration
	{
		public M6_ExtendArchiveWithExpirationAndGroup()
		{
		}

		public override void Down()
		{
			throw new NotSupportedException("Migrating down is not supported.");
		}

		public override void Up()
		{
			base.Alter.Table("Archive").AddColumn("Expires").AsDateTimeOffset().Nullable().AddColumn("GroupName").AsString().Nullable();
			int totalDays = (int)base.GetConfiguration<MaintenanceConfiguration>().CleanUpArchivesOlderThan.TotalDays;
			base.Execute.Sql(string.Concat("UPDATE Archive SET Expires = DATEADD(d, ", totalDays, ", Created)"));
		}
	}
}