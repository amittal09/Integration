using FluentMigrator;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Alter.Table;
using System;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	[Migration(4L)]
	public class M4_ExtendTaskLogWithExecutionContext : Migration
	{
		public M4_ExtendTaskLogWithExecutionContext()
		{
		}

		public override void Down()
		{
			throw new NotSupportedException("Migrating down is not supported.");
		}

		public override void Up()
		{
			base.Alter.Table("TaskLog").AddColumn("MachineName").AsString(255).Nullable().AddColumn("IdentityName").AsString(255).Nullable().AddColumn("CommandLine").AsString(2147483647).Nullable();
		}
	}
}