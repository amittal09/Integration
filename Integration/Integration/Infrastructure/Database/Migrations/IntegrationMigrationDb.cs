using Castle.MicroKernel;
using FluentMigrator.Runner;
using System;
using System.Reflection;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public class IntegrationMigrationDb : MigrationDb
	{
		internal IntegrationMigrationDb(Vertica.Integration.Infrastructure.Database.Migrations.DatabaseServer databaseServer, Vertica.Integration.Infrastructure.ConnectionString connectionString, System.Reflection.Assembly assembly, string namespaceContainingMigrations) : base(databaseServer, connectionString, assembly, namespaceContainingMigrations, "IntegrationDb")
		{
		}

		public MigrationDb CopyTo(Type type, string identifyingName)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return new MigrationDb(base.DatabaseServer, base.ConnectionString, type.Assembly, type.Namespace, identifyingName);
		}

		public override void List(MigrationRunner runner, ITaskExecutionContext context, IKernel kernel)
		{
		}

		public override void Rollback(MigrationRunner runner, ITaskExecutionContext log, IKernel kernel)
		{
		}
	}
}