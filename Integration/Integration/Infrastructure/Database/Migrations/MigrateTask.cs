using Castle.MicroKernel;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Versioning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.AttributeExt;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public class MigrateTask : Task
	{
		private readonly IKernel _kernel;

		private readonly MigrationDb[] _dbs;

		private readonly IDisposable _loggingDisabler;

		private readonly bool _databaseCreated;

		private readonly ITaskFactory _taskFactory;

		private readonly ITaskRunner _taskRunner;

		public override string Description
		{
			get
			{
				return "Runs migrations against all configured databases. Will also execute any custom task if provided by Arguments.";
			}
		}

		public MigrateTask(Func<Vertica.Integration.Infrastructure.Database.IDbFactory> db, ILogger logger, IKernel kernel, IMigrationDbs dbs, ITaskFactory taskFactory, ITaskRunner taskRunner)
		{
			StringBuilder stringBuilder;
			this._kernel = kernel;
			this._taskFactory = taskFactory;
			this._taskRunner = taskRunner;
			if (!dbs.IntegrationDbDisabled)
			{
				string str = MigrateTask.EnsureIntegrationDb(db(), dbs.CheckExistsAndCreateIntegrationDbIfNotFound, out this._databaseCreated);
				IntegrationMigrationDb integrationMigrationDb = new IntegrationMigrationDb(dbs.IntegrationDbDatabaseServer, ConnectionString.FromText(str), typeof(M1_Baseline).Assembly, typeof(M1_Baseline).Namespace);
				if (!this.CreateRunner(integrationMigrationDb, out stringBuilder).VersionLoader.VersionInfo.HasAppliedMigration(MigrateTask.FindLatestMigration()))
				{
					this._loggingDisabler = logger.Disable();
				}
				dbs = dbs.WithIntegrationDb(integrationMigrationDb);
			}
			this._dbs = dbs.ToArray<MigrationDb>();
		}

		private static IMigrationProcessorFactory CreateFactory(DatabaseServer databaseServer)
		{
			switch (databaseServer)
			{
				case DatabaseServer.SqlServer2012:
				{
					return new SqlServer2012ProcessorFactory();
				}
				case DatabaseServer.SqlServer2014:
				{
					return new SqlServer2014ProcessorFactory();
				}
				case DatabaseServer.Sqlite:
				{
					return new SQLiteProcessorFactory();
				}
			}
			throw new ArgumentOutOfRangeException("databaseServer");
		}

		private MigrationRunner CreateRunner(MigrationDb db, out StringBuilder output)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder1 = stringBuilder;
			output = stringBuilder;
			StringBuilder stringBuilder2 = stringBuilder1;
			TextWriterAnnouncer textWriterAnnouncer = new TextWriterAnnouncer((string s) => {
				if (stringBuilder2.Length == 0)
				{
					stringBuilder2.AppendLine();
				}
				stringBuilder2.Append(s);
			});
			IMigrationProcessor migrationProcessor = MigrateTask.CreateFactory(db.DatabaseServer).Create(db.ConnectionString, textWriterAnnouncer, new MigrateTask.MigrationOptions());
			RunnerContext runnerContext = new RunnerContext(textWriterAnnouncer)
			{
				Namespace = db.NamespaceContainingMigrations,
				ApplicationContext = this._kernel
			};
			return new MigrationRunner(db.Assembly, runnerContext, migrationProcessor);
		}

		public override void End(EmptyWorkItem workItem, ITaskExecutionContext context)
		{
			ITask task;
			if (this._databaseCreated)
			{
				context.Log.Warning(Target.Service, "Created new database (using Simple Recovery) and applied migrations to this. Make sure to configure this new database (auto growth, backup etc).", new object[0]);
			}
			string[] strArrays = (context.Arguments["Tasks"] ?? string.Empty).Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (!this._taskFactory.TryGet(str, out task))
				{
					context.Log.Warning(Target.Service, "Task with name '{0}' not found.", new object[] { str });
				}
				else if (!(task is MigrateTask))
				{
					this._taskRunner.Execute(task, null);
				}
			}
		}

		private static string EnsureIntegrationDb(Vertica.Integration.Infrastructure.Database.IDbFactory db, bool checkExistsAndCreateIntegrationDbIfNotFound, out bool databaseCreated)
		{
			string connectionString;
			using (IDbConnection connection = db.GetConnection())
			{
				if (checkExistsAndCreateIntegrationDbIfNotFound)
				{
					using (IDbCommand dbCommand = connection.CreateCommand())
					{
						string database = connection.Database;
						SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);
						Func<string, string> item = (string dbName) => {
							sqlConnectionStringBuilder["Initial Catalog"] = dbName;
							return sqlConnectionStringBuilder.ConnectionString;
						};
						connection.ConnectionString = item("master");
						connection.Open();
						dbCommand.CommandText = "\r\nIF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = @DbName)\r\n\tBEGIN\r\n\r\n\t\tEXECUTE ('\r\n\t\t\tCREATE DATABASE ' + @DbName + ';\r\n\t\t\tALTER DATABASE ' + @DbName +' SET RECOVERY SIMPLE\r\n\t\t')\r\n\t\t\r\n\t\tSELECT 'CREATED'\r\n\tEND\r\nELSE\r\n\tSELECT 'EXISTS'\r\n";
						IDbDataParameter dbDataParameter = dbCommand.CreateParameter();
						dbDataParameter.ParameterName = "DbName";
						dbDataParameter.Value = database;
						dbCommand.Parameters.Add(dbDataParameter);
						databaseCreated = (string)dbCommand.ExecuteScalar() == "CREATED";
						connectionString = item(database);
					}
				}
				else
				{
					databaseCreated = false;
					connectionString = connection.ConnectionString;
				}
			}
			return connectionString;
		}

		private static long FindLatestMigration()
		{
			return ((IEnumerable<Type>)typeof(M1_Baseline).Assembly.GetTypes()).Where<Type>((Type x) => {
				if (!x.IsClass)
				{
					return false;
				}
				return x.Namespace == typeof(M1_Baseline).Namespace;
			}).Select<Type, long>((Type x) => {
				MigrationAttribute attribute = x.GetAttribute<MigrationAttribute>(false);
				if (attribute == null)
				{
					return (long)-1;
				}
				return attribute.Version;
			}).OrderByDescending<long, long>((long x) => x).First<long>();
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			StringBuilder stringBuilder;
			bool flag = this._loggingDisabler != null;
			MigrationDb[] array = this._dbs;
			string[] strArrays = (context.Arguments["Names"] ?? string.Empty).Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
			if (strArrays.Length != 0)
			{
				ILookup<string, MigrationDb> lookup = ((IEnumerable<MigrationDb>)this._dbs).ToLookup<MigrationDb, string, MigrationDb>((MigrationDb db) => db.IdentifyingName, (MigrationDb db) => db, StringComparer.OrdinalIgnoreCase);
				array = strArrays.SelectMany<string, MigrationDb>((string x) => lookup[x]).ToArray<MigrationDb>();
			}
			string lowerInvariant = (context.Arguments["Action"] ?? string.Empty).ToLowerInvariant();
			MigrationDb[] migrationDbArray = array;
			for (int i = 0; i < (int)migrationDbArray.Length; i++)
			{
				MigrationDb migrationDb = migrationDbArray[i];
				MigrationRunner migrationRunner = this.CreateRunner(migrationDb, out stringBuilder);
				if (lowerInvariant == "list")
				{
					migrationDb.List(migrationRunner, context, this._kernel);
				}
				else if (lowerInvariant == "rollback")
				{
					migrationDb.Rollback(migrationRunner, context, this._kernel);
				}
				else
				{
					migrationDb.MigrateUp(migrationRunner, context, this._kernel);
				}
				migrationRunner.Processor.Dispose();
				if (stringBuilder.Length > 0)
				{
					context.Log.Message(stringBuilder.ToString(), new object[0]);
				}
				if (flag)
				{
					this._loggingDisabler.Dispose();
					flag = false;
				}
			}
		}

		private class MigrationOptions : IMigrationProcessorOptions
		{
			public bool PreviewOnly
			{
				get;
			}

			public string ProviderSwitches
			{
				get;
			}

			public int Timeout
			{
				get;
			}

			public MigrationOptions()
			{
				this.PreviewOnly = false;
				this.ProviderSwitches = null;
				this.Timeout = 60;
			}
		}
	}
}