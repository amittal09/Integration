using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Database
{
	public class DatabaseConfiguration : IInitializable<IWindsorContainer>
	{
		private DefaultConnection _defaultConnection;

		public ApplicationConfiguration Application
		{
			get;
		}

		public bool IntegrationDbDisabled
		{
			get;
			private set;
		}

		internal DatabaseConfiguration(ApplicationConfiguration application)
		{
			if (application == null)
			{
				throw new ArgumentNullException("application");
			}
			this.Application = application;
		}

		public DatabaseConfiguration AddConnection<TConnection>(TConnection connection)
		where TConnection : Connection
		{
			this.Application.AddCustomInstaller(new DbInstaller<TConnection>(connection, false));
			return this;
		}

		public DatabaseConfiguration Change(Action<DatabaseConfiguration> change)
		{
			if (change != null)
			{
				change(this);
			}
			return this;
		}

		public DatabaseConfiguration DisableIntegrationDb()
		{
			this.IntegrationDbDisabled = true;
			return this;
		}

		public DatabaseConfiguration IntegrationDb(ConnectionString connectionString)
		{
			if (connectionString == null)
			{
				throw new ArgumentNullException("connectionString");
			}
			this._defaultConnection = new DefaultConnection(connectionString);
			return this;
		}

		public DatabaseConfiguration IntegrationDb(Connection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			this._defaultConnection = new DefaultConnection(connection);
			return this;
		}

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
			container.Install(new IWindsorInstaller[] { new DbInstaller((this.IntegrationDbDisabled ? DefaultConnection.Disabled : this._defaultConnection ?? new DefaultConnection(ConnectionString.FromName("IntegrationDb")))) });
		}
	}
}