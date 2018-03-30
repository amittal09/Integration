using Castle.MicroKernel;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public class MigrationDb : IEquatable<MigrationDb>
	{
		public System.Reflection.Assembly Assembly
		{
			get;
		}

		public Vertica.Integration.Infrastructure.ConnectionString ConnectionString
		{
			get;
		}

		public Vertica.Integration.Infrastructure.Database.Migrations.DatabaseServer DatabaseServer
		{
			get;
			private set;
		}

		public string IdentifyingName
		{
			get;
			private set;
		}

		public string NamespaceContainingMigrations
		{
			get;
		}

		public MigrationDb(Vertica.Integration.Infrastructure.Database.Migrations.DatabaseServer databaseServer, Vertica.Integration.Infrastructure.ConnectionString connectionString, System.Reflection.Assembly assembly, string namespaceContainingMigrations, string identifyingName = null)
		{
			if (connectionString == null)
			{
				throw new ArgumentNullException("connectionString");
			}
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (string.IsNullOrWhiteSpace(namespaceContainingMigrations))
			{
				throw new ArgumentException("Value cannot be null or empty.", "namespaceContainingMigrations");
			}
			this.DatabaseServer = databaseServer;
			this.ConnectionString = connectionString;
			this.Assembly = assembly;
			this.NamespaceContainingMigrations = namespaceContainingMigrations;
			this.IdentifyingName = identifyingName.NullIfEmpty() ?? namespaceContainingMigrations;
		}

		public bool Equals(MigrationDb other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (!object.Equals(this.Assembly, other.Assembly) || !string.Equals(this.NamespaceContainingMigrations, other.NamespaceContainingMigrations))
			{
				return false;
			}
			return object.Equals(this.ConnectionString, other.ConnectionString);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			return this.Equals((MigrationDb)obj);
		}

		public override int GetHashCode()
		{
			return ((this.Assembly != null ? this.Assembly.GetHashCode() : 0) * 397 ^ (this.NamespaceContainingMigrations != null ? this.NamespaceContainingMigrations.GetHashCode() : 0)) * 397 ^ (this.ConnectionString != null ? this.ConnectionString.GetHashCode() : 0);
		}

		public virtual void List(MigrationRunner runner, ITaskExecutionContext context, IKernel kernel)
		{
			if (runner == null)
			{
				throw new ArgumentNullException("runner");
			}
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			runner.ListMigrations();
		}

		public virtual void MigrateUp(MigrationRunner runner, ITaskExecutionContext context, IKernel kernel)
		{
			if (runner == null)
			{
				throw new ArgumentNullException("runner");
			}
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			runner.MigrateUp();
		}

		public virtual void Rollback(MigrationRunner runner, ITaskExecutionContext context, IKernel kernel)
		{
			if (runner == null)
			{
				throw new ArgumentNullException("runner");
			}
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			if (runner.RunnerContext.Steps == 0)
			{
				runner.RunnerContext.Steps = 1;
			}
			runner.Rollback(runner.RunnerContext.Steps);
		}
	}
}