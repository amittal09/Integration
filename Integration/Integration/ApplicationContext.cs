using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration
{
	public sealed class ApplicationContext : IApplicationContext, IDisposable
	{
		private readonly static Lazy<Action> EnsureSingleton;

		private readonly ApplicationConfiguration _configuration;

		private readonly IWindsorContainer _container;

		private readonly IArgumentsParser _parser;

		private readonly IHost[] _hosts;

		private readonly Lazy<Action> _disposed = new Lazy<Action>(new Func<Action>(() => () => {
		}));

		static ApplicationContext()
		{
			ApplicationContext.EnsureSingleton = new Lazy<Action>(() => () => {
			});
		}

		internal ApplicationContext(Action<ApplicationConfiguration> application)
		{
			this._configuration = new ApplicationConfiguration();
			if (application != null)
			{
				application(this._configuration);
			}
			this._configuration.RegisterDependency<IApplicationContext>(this);
			if (this._configuration.IgnoreSslErrors)
			{
				ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
			}
			this._container = CastleWindsor.Initialize(this._configuration);
			this._parser = this.Resolve<IArgumentsParser>();
			this._hosts = this.Resolve<IHostFactory>().GetAll();
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.LogException);
		}

		public static IApplicationContext Create(Action<ApplicationConfiguration> application = null)
		{
			if (ApplicationContext.EnsureSingleton.IsValueCreated)
			{
				throw new InvalidOperationException("An instance of ApplicationContext has already been created. It might have been disposed, but you should make sure to reuse the same instance for the entire lifecycle of this application.");
			}
			ApplicationContext.EnsureSingleton.Value();
			return new ApplicationContext(application);
		}

		public void Dispose()
		{
			if (this._disposed.IsValueCreated)
			{
				throw new InvalidOperationException("ApplicationContext has already been disposed.");
			}
			this._disposed.Value();
			this._configuration.Extensibility((ExtensibilityConfiguration extensibility) => {
				foreach (IDisposable disposable in extensibility.OfType<IDisposable>())
				{
					try
					{
						disposable.Dispose();
					}
					catch (Exception exception)
					{
						this.LogException(exception);
					}
				}
			});
			AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(this.LogException);
			this._container.Dispose();
		}

		public void Execute(params string[] args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			this.Execute(this._parser.Parse(args));
		}

		public void Execute(HostArguments args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			IHost[] array = (
				from x in this._hosts
				where x.CanHandle(args)
				select x).ToArray<IHost>();
			if (array.Length == 0)
			{
				throw new NoHostFoundException(args);
			}
			if ((int)array.Length > 1)
			{
				throw new MultipleHostsFoundException(args, array);
			}
			array[0].Handle(args);
		}

		private void LogException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception exceptionObject = e.ExceptionObject as Exception;
			if (exceptionObject == null)
			{
				return;
			}
			this.LogException(exceptionObject);
		}

		private void LogException(Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception");
			}
			if (exception is TaskExecutionFailedException)
			{
				return;
			}
			ILogger logger = this.Resolve<ILogger>();
			try
			{
				logger.LogError(exception, null);
			}
			catch
			{
				if (logger is EventLogger)
				{
					throw;
				}
				(new EventLogger(this.Resolve<EventLoggerConfiguration>(), this.Resolve<IRuntimeSettings>())).LogError(exception, null);
			}
		}

		public T Resolve<T>()
		{
			return this._container.Resolve<T>();
		}

		public T[] ResolveAll<T>()
		{
			return this._container.ResolveAll<T>();
		}
	}
}