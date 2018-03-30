using FluentMigrator;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using System;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	[Migration(2L)]
	public class M2_Archive : Migration
	{
		public M2_Archive()
		{
		}

		public override void Down()
		{
			throw new NotSupportedException("Migrating down is not supported.");
		}

		public override void Up()
		{
			base.Create.Table("Archive").WithColumn("Id").AsInt32().PrimaryKey().Identity().WithColumn("Name").AsString(255).WithColumn("BinaryData").AsBinary(2147483647).WithColumn("ByteSize").AsInt32().WithColumn("Created").AsDateTimeOffset();
		}
	}
}