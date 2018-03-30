using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration
{
	public class ApplicationConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly ExtensibilityConfiguration _extensibility;

		private readonly List<IWindsorInstaller> _customInstallers;

		private readonly DatabaseConfiguration _database;

		private readonly TasksConfiguration _tasks;

		private readonly LoggingConfiguration _logging;

		private readonly MigrationConfiguration _migration;

		private readonly HostsConfiguration _hosts;

		private readonly AdvancedConfiguration _advanced;

		public bool IgnoreSslErrors
		{
			get;
			set;
		}

		internal ApplicationConfiguration()
		{
			this._extensibility = new ExtensibilityConfiguration();
			this.IgnoreSslErrors = true;
			this._customInstallers = new List<IWindsorInstaller>();
			this._database = this.Register<DatabaseConfiguration>(() => new DatabaseConfiguration(this));
			this._tasks = this.Register<TasksConfiguration>(() => new TasksConfiguration(this));
			this._logging = this.Register<LoggingConfiguration>(() => new LoggingConfiguration(this));
			this._migration = this.Register<MigrationConfiguration>(() => new MigrationConfiguration(this));
			this._hosts = this.Register<HostsConfiguration>(() => new HostsConfiguration(this));
			this._advanced = this.Register<AdvancedConfiguration>(() => new AdvancedConfiguration(this));
			this.Register<ApplicationConfiguration>(() => this);
		}

		public ApplicationConfiguration AddCustomInstaller(IWindsorInstaller installer)
		{
			return this.AddCustomInstallers(new IWindsorInstaller[] { installer });
		}

		public ApplicationConfiguration AddCustomInstallers(params IWindsorInstaller[] installers)
		{
			if (installers != null)
			{
				this._customInstallers.AddRange(installers.SkipNulls<IWindsorInstaller>());
			}
			return this;
		}

		public ApplicationConfiguration Advanced(Action<AdvancedConfiguration> advanced)
		{
			if (advanced != null)
			{
				advanced(this._advanced);
			}
			return this;
		}

		public ApplicationConfiguration Change(Action<ApplicationConfiguration> change)
		{
			if (change != null)
			{
				change(this);
			}
			return this;
		}

		public ApplicationConfiguration Database(Action<DatabaseConfiguration> database)
		{
			if (database != null)
			{
				database(this._database);
			}
			return this;
		}

		public ApplicationConfiguration Extensibility(Action<ExtensibilityConfiguration> extensibility)
		{
			if (extensibility != null)
			{
				extensibility(this._extensibility);
			}
			return this;
		}

		public ApplicationConfiguration Hosts(Action<HostsConfiguration> hosts)
		{
			if (hosts != null)
			{
				hosts(this._hosts);
			}
			return this;
		}

		public ApplicationConfiguration Logging(Action<LoggingConfiguration> logging)
		{
			if (logging != null)
			{
				logging(this._logging);
			}
			return this;
		}

		public ApplicationConfiguration Migration(Action<MigrationConfiguration> migration)
		{
			if (migration != null)
			{
				migration(this._migration);
			}
			return this;
		}

		private T Register<T>(Func<T> factory)
		where T : class
		{
			T t = default(T);
			this.Extensibility((ExtensibilityConfiguration extensibility) => t = extensibility.Register<T>(factory));
			return t;
		}

		public ApplicationConfiguration RegisterDependency<T>(T singletonInstance)
		where T : class
		{
			if (singletonInstance == null)
			{
				throw new ArgumentNullException("singletonInstance");
			}
			return this.AddCustomInstaller(new InstanceInstaller<T>(singletonInstance));
		}

		public ApplicationConfiguration RuntimeSettings<T>()
		where T : IRuntimeSettings
		{
			return this.Advanced((AdvancedConfiguration advanced) => advanced.Register<IRuntimeSettings, T>());
		}

		public ApplicationConfiguration RuntimeSettings<T>(T instance)
		where T : IRuntimeSettings
		{
			Func<IRuntimeSettings> func2 = null;
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			return this.Advanced((AdvancedConfiguration advanced) => {
				AdvancedConfiguration advancedConfiguration = advanced;
				Func<IRuntimeSettings> u003cu003e9_1 = func2;
				//if (u003cu003e9_1 == null)
				//{
				//	Func<IRuntimeSettings> func = () => (object)instance;
				//	Func<IRuntimeSettings> func1 = func;
				//	func2 = func;
				//	u003cu003e9_1 = func1;
				//}
				advancedConfiguration.Register<IRuntimeSettings>(u003cu003e9_1);
			});
		}

		public ApplicationConfiguration Tasks(Action<TasksConfiguration> tasks)
		{
			if (tasks != null)
			{
				tasks(this._tasks);
			}
			return this;
		}

		void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.Install(this._customInstallers.ToArray());
		}
	}
}