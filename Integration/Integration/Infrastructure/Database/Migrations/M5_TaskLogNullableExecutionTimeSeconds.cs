using FluentMigrator;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Execute;
using System;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	[Migration(5L)]
	public class M5_TaskLogNullableExecutionTimeSeconds : Migration
	{
		public M5_TaskLogNullableExecutionTimeSeconds()
		{
		}

		public override void Down()
		{
			throw new NotSupportedException("Migrating down is not supported.");
		}

		public override void Up()
		{
			base.Alter.Table("TaskLog").AlterColumn("ExecutionTimeSeconds").AsDouble().Nullable();
			base.Execute.Sql("UPDATE TaskLog SET ExecutionTimeSeconds = NULL WHERE ([Type] = 'M');");
		}
	}
}