using Castle.Windsor;
using FluentMigrator;
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
        where T : Migration
        {
            this._dbs.Add(typeof(T), identifyingName);
            return this;
        }

        public MigrationConfiguration AddFromNamespaceOfThis<T>(DatabaseServer db, ConnectionString connectionString, string identifyingName = null)
        where T : Migration
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

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            this.Application.Database((DatabaseConfiguration database) => this._dbs.IntegrationDbDisabled = database.IntegrationDbDisabled);
            container.RegisterInstance<IMigrationDbs>(this._dbs);
        }

        private class MigrationDbs : IMigrationDbs, IEnumerable<MigrationDb>, IEnumerable
        {
            private readonly List<MigrationDb> _dbs;

            private readonly List<Tuple<Type, string>> _types;

            public bool CheckExistsAndCreateIntegrationDbIfNotFound
            {
                get
                {
                    return JustDecompileGenerated_get_CheckExistsAndCreateIntegrationDbIfNotFound();
                }
                set
                {
                    JustDecompileGenerated_set_CheckExistsAndCreateIntegrationDbIfNotFound(value);
                }
            }

            private bool JustDecompileGenerated_CheckExistsAndCreateIntegrationDbIfNotFound_k__BackingField;

            public bool JustDecompileGenerated_get_CheckExistsAndCreateIntegrationDbIfNotFound()
            {
                return this.JustDecompileGenerated_CheckExistsAndCreateIntegrationDbIfNotFound_k__BackingField;
            }

            public void JustDecompileGenerated_set_CheckExistsAndCreateIntegrationDbIfNotFound(bool value)
            {
                this.JustDecompileGenerated_CheckExistsAndCreateIntegrationDbIfNotFound_k__BackingField = value;
            }

            public DatabaseServer IntegrationDbDatabaseServer
            {
                get
                {
                    return JustDecompileGenerated_get_IntegrationDbDatabaseServer();
                }
                set
                {
                    JustDecompileGenerated_set_IntegrationDbDatabaseServer(value);
                }
            }

            private DatabaseServer JustDecompileGenerated_IntegrationDbDatabaseServer_k__BackingField;

            public DatabaseServer JustDecompileGenerated_get_IntegrationDbDatabaseServer()
            {
                return this.JustDecompileGenerated_IntegrationDbDatabaseServer_k__BackingField;
            }

            public void JustDecompileGenerated_set_IntegrationDbDatabaseServer(DatabaseServer value)
            {
                this.JustDecompileGenerated_IntegrationDbDatabaseServer_k__BackingField = value;
            }

            public bool IntegrationDbDisabled
            {
                get
                {
                    return JustDecompileGenerated_get_IntegrationDbDisabled();
                }
                set
                {
                    JustDecompileGenerated_set_IntegrationDbDisabled(value);
                }
            }

            private bool JustDecompileGenerated_IntegrationDbDisabled_k__BackingField;

            public bool JustDecompileGenerated_get_IntegrationDbDisabled()
            {
                return this.JustDecompileGenerated_IntegrationDbDisabled_k__BackingField;
            }

            public void JustDecompileGenerated_set_IntegrationDbDisabled(bool value)
            {
                this.JustDecompileGenerated_IntegrationDbDisabled_k__BackingField = value;
            }

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