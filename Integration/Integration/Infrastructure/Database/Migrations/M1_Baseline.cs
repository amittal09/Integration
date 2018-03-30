using FluentMigrator;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using System;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	[Migration(1L)]
	public class M1_Baseline : Migration
	{
		public M1_Baseline()
		{
		}

		public override void Down()
		{
			throw new NotSupportedException("Migrating down is not supported.");
		}

		public override void Up()
		{
			base.Create.Table("ErrorLog").WithColumn("Id").AsInt32().PrimaryKey().Identity().WithColumn("MachineName").AsString(255).WithColumn("IdentityName").AsString(255).Nullable().WithColumn("CommandLine").AsString(2147483647).Nullable().WithColumn("Message").AsString(2147483647).Nullable().WithColumn("FormattedMessage").AsCustom("NTEXT").Nullable().WithColumn("Severity").AsString(50).WithColumn("Target").AsString(50).WithColumn("TimeStamp").AsDateTimeOffset();
			base.Create.Table("TaskLog").WithColumn("Id").AsInt32().PrimaryKey().Identity().WithColumn("Type").AsAnsiString(1).WithColumn("TaskName").AsString(255).WithColumn("StepName").AsString(255).Nullable().WithColumn("Message").AsString(2147483647).Nullable().WithColumn("ExecutionTimeSeconds").AsDouble().WithColumn("TimeStamp").AsDateTimeOffset().WithColumn("TaskLog_Id").AsInt32().Nullable().WithColumn("StepLog_Id").AsInt32().Nullable().WithColumn("ErrorLog_Id").AsInt32().Nullable();
		}
	}
}