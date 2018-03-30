using Castle.MicroKernel.Registration;
using System.Linq;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Infrastructure
{
	public class AdvancedConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly IDictionary<Type, Tuple<Type, Type>> _types;

		private readonly IDictionary<Type, Tuple<Func<object>, Func<object>>> _instances;

		private ProcessExitConfiguration _processExit;

		public ApplicationConfiguration Application
		{
			get;
		}

		internal AdvancedConfiguration(ApplicationConfiguration application)
		{
			if (application == null)
			{
				throw new ArgumentNullException("application");
			}
			this.Application = application;
			this._types = new Dictionary<Type, Tuple<Type, Type>>();
			this._instances = new Dictionary<Type, Tuple<Func<object>, Func<object>>>();
			this.Register<ILogger, DbLogger, EventLogger>();
			this.Register<IConfigurationRepository, DbConfigurationRepository, FileBasedConfigurationRepository>();
			this.Register<IArchiveService, DbArchiveService, FileBasedArchiveService>();
			this.Register<IRuntimeSettings, AppConfigRuntimeSettings>();
			this.Register<TextWriter>(() => {
				if (!Environment.UserInteractive)
				{
					return TextWriter.Null;
				}
				return Console.Out;
			});
			this.Application.Extensibility((ExtensibilityConfiguration extensibility) => this._processExit = extensibility.Register<ProcessExitConfiguration>(() => new ProcessExitConfiguration(this)));
		}

		public AdvancedConfiguration ProcessExit(Action<ProcessExitConfiguration> processExit)
		{
			if (processExit == null)
			{
				throw new ArgumentNullException("processExit");
			}
			processExit(this._processExit);
			return this;
		}

		public AdvancedConfiguration Register<TService, TImpl>()
		where TService : class
		where TImpl : TService
		{
			return this.Register<TService, TImpl, TImpl>();
		}

		public AdvancedConfiguration Register<TService, TIntegrationDbEnabledImpl, TIntegrationDbDisabledImpl>()
		where TService : class
		where TIntegrationDbEnabledImpl : TService
		where TIntegrationDbDisabledImpl : TService
		{
			this._types[typeof(TService)] = Tuple.Create<Type, Type>(typeof(TIntegrationDbEnabledImpl), typeof(TIntegrationDbDisabledImpl));
			this._instances[typeof(TService)] = null;
			return this;
		}

		public AdvancedConfiguration Register<TService>(Func<TService> instance)
		where TService : class
		{
			return this.Register<TService>(instance, instance);
		}

		public AdvancedConfiguration Register<TService>(Func<TService> integrationDbEnabledInstance, Func<TService> integrationDbDisabledInstance)
		where TService : class
		{
			if (integrationDbEnabledInstance == null)
			{
				throw new ArgumentNullException("integrationDbEnabledInstance");
			}
			if (integrationDbDisabledInstance == null)
			{
				throw new ArgumentNullException("integrationDbDisabledInstance");
			}
			this._instances[typeof(TService)] = Tuple.Create<Func<object>, Func<object>>(integrationDbEnabledInstance, integrationDbDisabledInstance);
			this._types[typeof(TService)] = null;
			return this;
		}

		void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			bool integrationDbDisabled = false;
			this.Application.Database((DatabaseConfiguration database) => integrationDbDisabled = database.IntegrationDbDisabled);
			foreach (KeyValuePair<Type, Tuple<Type, Type>> keyValuePair in 
				from x in this._types
				where x.Value != null
				select x)
			{
				container.Register(new IRegistration[] { Component.For(keyValuePair.Key).ImplementedBy((!integrationDbDisabled ? keyValuePair.Value.Item1 : keyValuePair.Value.Item2)) });
			}
			foreach (KeyValuePair<Type, Tuple<Func<object>, Func<object>>> keyValuePair1 in 
				from x in this._instances
				where x.Value != null
				select x)
			{
				container.Register(new IRegistration[] { Component.For(keyValuePair1.Key).Instance((!integrationDbDisabled ? keyValuePair1.Value.Item1() : keyValuePair1.Value.Item2())) });
			}
		}
	}
}