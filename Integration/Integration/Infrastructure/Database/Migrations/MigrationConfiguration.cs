using Castle.Windsor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public class MigrationConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly MigrationConfiguration.MigrationDbs _dbs;

		public ApplicationConfiguration Application
		{
			get;
		}

		internal MigrationConfiguration(ApplicationConfiguration application)
		{
			if (application == null)
			{
				throw new ArgumentNullException("application");
			}
			this.Application = application;
			this._dbs = new MigrationConfiguration.MigrationDbs();
		}

		public MigrationConfiguration Add(MigrationDb migrationDb)
		{
			if (migrationDb == null)
			{
				throw new ArgumentNullException("migrationDb");
			}
			this._dbs.Add(migrationDb);
			return this;
		}

		public MigrationConfiguration AddFromNamespaceOfThis<T>(string identifyingName = null)
		where T : class//Migration
		{
			this._dbs.Add(typeof(T), identifyingName);
			return this;
		}

		public MigrationConfiguration AddFromNamespaceOfThis<T>(DatabaseServer db, ConnectionString connectionString, string identifyingName = null)
		where T :class// Migration
		{
			if (connectionString == null)
			{
				throw new ArgumentNullException("connectionString");
			}
			return this.Add(new MigrationDb(db, connectionString, typeof(T).Assembly, typeof(T).Namespace, identifyingName));
		}

		public MigrationConfiguration ChangeIntegrationDbDatabaseServer(DatabaseServer db)
		{
			this._dbs.IntegrationDbDatabaseServer = db;
			return this;
		}

		public MigrationConfiguration DisableCheckExistsAndCreateIntegrationDbIfNotFound()
		{
			this._dbs.CheckExistsAndCreateIntegrationDbIfNotFound = false;
			return this;
		}

		//void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		//{
		//	this.Application.Database((DatabaseConfiguration database) => this._dbs.IntegrationDbDisabled = database.IntegrationDbDisabled);
		//	container.RegisterInstance<IMigrationDbs>(this._dbs);
		//}

        public void Initialize(IWindsorContainer context)
        {
            throw new NotImplementedException();
        }

        private class MigrationDbs : IMigrationDbs, IEnumerable<MigrationDb>, IEnumerable
		{
			private readonly List<MigrationDb> _dbs;

			private readonly List<Tuple<Type, string>> _types;

			public bool CheckExistsAndCreateIntegrationDbIfNotFound
			{
				get
				{
                    return this._CheckExistsAndCreateIntegrationDbIfNotFound;
                }
				set
				{
                    this._CheckExistsAndCreateIntegrationDbIfNotFound=value;
                }
			}

			private bool _CheckExistsAndCreateIntegrationDbIfNotFound;

			//public bool get_CheckExistsAndCreateIntegrationDbIfNotFound()
			//{
			//	return this.CheckExistsAndCreateIntegrationDbIfNotFound;
			//}

			//public void set_CheckExistsAndCreateIntegrationDbIfNotFound(bool value)
			//{
			//	this.CheckExistsAndCreateIntegrationDbIfNotFound = value;
			//}

			public DatabaseServer IntegrationDbDatabaseServer
			{
				get
				{
					return this._IntegrationDbDatabaseServer;
				}
				set
				{
                    this._IntegrationDbDatabaseServer = value;
				}
			}

			private DatabaseServer _IntegrationDbDatabaseServer;

			
			
			public bool IntegrationDbDisabled
			{
				get
				{
					return this._IntegrationDbDisabled;
				}
				set
				{
                    this._IntegrationDbDisabled = value;
				}
			}

			private bool _IntegrationDbDisabled;


			public MigrationDbs()
			{
				this._dbs = new List<MigrationDb>();
				this._types = new List<Tuple<Type, string>>();
				this.IntegrationDbDatabaseServer = DatabaseServer.SqlServer2014;
				this.CheckExistsAndCreateIntegrationDbIfNotFound = true;
			}

			public void Add(MigrationDb migrationDb)
			{
				if (migrationDb == null)
				{
					throw new ArgumentNullException("migrationDb");
				}
				this._dbs.Add(migrationDb);
			}

			public void Add(Type migration, string identifyingName)
			{
				if (migration == null)
				{
					throw new ArgumentNullException("migration");
				}
				this._types.Add(Tuple.Create<Type, string>(migration, identifyingName));
			}

			public IEnumerator<MigrationDb> GetEnumerator()
			{
				return this._dbs.Distinct<MigrationDb>().GetEnumerator();
			}

			IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			public IMigrationDbs WithIntegrationDb(IntegrationMigrationDb integrationDb)
			{
				if (integrationDb == null)
				{
					throw new ArgumentNullException("integrationDb");
				}
				if (this.IntegrationDbDisabled)
				{
					throw new InvalidOperationException("IntegrationDb is disabled.");
				}
				this._dbs.Insert(0, integrationDb);
				foreach (Tuple<Type, string> _type in this._types)
				{
					this._dbs.Insert(1, integrationDb.CopyTo(_type.Item1, _type.Item2));
				}
				return this;
			}
		}
	}
}