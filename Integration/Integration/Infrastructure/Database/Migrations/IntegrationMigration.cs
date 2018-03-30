using Castle.MicroKernel;
using FluentMigrator;
using System;
using System.Collections.Specialized;
using System.Configuration;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public abstract class IntegrationMigration : Migration
	{
		protected NameValueCollection AppSettings
		{
			get
			{
				return ConfigurationManager.AppSettings;
			}
		}

		protected IConfigurationRepository ConfigurationRepository
		{
			get
			{
				return this.Resolve<IConfigurationRepository>();
			}
		}

		protected IConfigurationService ConfigurationService
		{
			get
			{
				return this.Resolve<IConfigurationService>();
			}
		}

		private IKernel Kernel
		{
			get
			{
				return base.ApplicationContext as IKernel;
			}
		}

		protected IRuntimeSettings RuntimeSettings
		{
			get
			{
				return this.Resolve<IRuntimeSettings>();
			}
		}

		protected IntegrationMigration()
		{
		}

		public override void Down()
		{
		}

		protected T GetConfiguration<T>()
		where T : class, new()
		{
			return this.ConfigurationService.Get<T>();
		}

		protected Vertica.Integration.Infrastructure.Configuration.Configuration GetRawConfiguration(string id)
		{
			return this.ConfigurationRepository.Get(id);
		}

		protected ITask GetTask(string name)
		{
			return this.Resolve<ITaskFactory>().Get(name);
		}

		protected ITask GetTask<TTask>()
		where TTask : class, ITask
		{
			return this.Resolve<ITaskFactory>().Get<TTask>();
		}

		protected void MergeConfiguration<T>(string id)
		where T : class, new()
		{
			Vertica.Integration.Infrastructure.Configuration.Configuration rawConfiguration = this.GetRawConfiguration(id);
			if (rawConfiguration != null)
			{
				this.GetConfiguration<T>();
				Vertica.Integration.Infrastructure.Configuration.Configuration jsonData = this.GetRawConfiguration(Vertica.Integration.Infrastructure.Configuration.ConfigurationService.GetGuidId<T>());
				jsonData.JsonData = rawConfiguration.JsonData;
				this.SaveRawConfiguration(jsonData);
				this.ConfigurationRepository.Delete(rawConfiguration.Id);
			}
		}

		public T Resolve<T>()
		{
			return this.Kernel.Resolve<T>();
		}

		protected void RunExecute(HostArguments args)
		{
			this.Resolve<IApplicationContext>().Execute(args);
		}

		protected void RunExecute(params string[] args)
		{
			this.Resolve<IApplicationContext>().Execute(args);
		}

		protected void RunTask(string name, Vertica.Integration.Model.Arguments arguments = null)
		{
			this.RunTask(this.GetTask(name), arguments);
		}

		protected void RunTask<TTask>(Vertica.Integration.Model.Arguments arguments = null)
		where TTask : class, ITask
		{
			this.RunTask(this.GetTask<TTask>(), arguments);
		}

		protected void RunTask(ITask task, Vertica.Integration.Model.Arguments arguments = null)
		{
			this.Resolve<ITaskRunner>().Execute(task, arguments ?? new Vertica.Integration.Model.Arguments());
		}

		protected void SaveConfiguration<T>(T configuration)
		where T : class, new()
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			this.ConfigurationService.Save<T>(configuration, "Migration", true);
		}

		protected void SaveRawConfiguration(Vertica.Integration.Infrastructure.Configuration.Configuration rawConfiguration)
		{
			if (rawConfiguration == null)
			{
				throw new ArgumentNullException("rawConfiguration");
			}
			this.ConfigurationService.Save<Vertica.Integration.Infrastructure.Configuration.Configuration>(rawConfiguration, "Migration", true);
		}

		protected ConfigurationUpdater<T> UpdateConfiguration<T>()
		where T : class, new()
		{
			return new ConfigurationUpdater<T>(this.GetConfiguration<T>(), new Action<T>(this.SaveConfiguration<T>));
		}
	}
}