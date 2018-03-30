using FluentMigrator;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using System;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	[Migration(3L)]
	public class M3_Configuration : Migration
	{
		public M3_Configuration()
		{
		}

		public override void Down()
		{
			throw new NotSupportedException("Migrating down is not supported.");
		}

		public override void Up()
		{
			base.Create.Table("Configuration").WithColumn("Id").AsString(255).PrimaryKey().WithColumn("Name").AsString(50).WithColumn("Description").AsString(255).Nullable().WithColumn("JsonData").AsString(2147483647).WithColumn("Created").AsDateTimeOffset().WithColumn("Updated").AsDateTimeOffset().WithColumn("UpdatedBy").AsString(50);
		}
	}
}