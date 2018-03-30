using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Infrastructure
{
	public class ProcessExitConfiguration : IInitializable<IWindsorContainer>
	{
		private IProcessExitHandler _customInstance;

		private Type _customType;

		public AdvancedConfiguration Advanced
		{
			get;
		}

		public ProcessExitConfiguration(AdvancedConfiguration advanced)
		{
			if (advanced == null)
			{
				throw new ArgumentNullException("advanced");
			}
			this.Advanced = advanced;
		}

		public ProcessExitConfiguration Custom<TCustomHandler>()
		where TCustomHandler : class, IProcessExitHandler
		{
			this._customInstance = null;
			this._customType = typeof(TCustomHandler);
			return this;
		}

		public ProcessExitConfiguration Custom(IProcessExitHandler customHandler)
		{
			if (customHandler == null)
			{
				throw new ArgumentNullException("customHandler");
			}
			this._customInstance = customHandler;
			this._customType = null;
			return this;
		}

		void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			if (this._customType != null)
			{
				container.Install(new IWindsorInstaller[] { Install.Type<IProcessExitHandler>(this._customType, null) });
				return;
			}
			if (this._customInstance != null)
			{
				container.Install(new IWindsorInstaller[] { Install.Instance<IProcessExitHandler>(this._customInstance) });
				return;
			}
			if (AzureWebJobShutdownHandler.IsRunningOnAzure())
			{
				container.Install(new IWindsorInstaller[] { Install.Service<IProcessExitHandler, AzureWebJobShutdownHandler>((Action<ComponentRegistration<IProcessExitHandler>>)null) });
				return;
			}
			container.Install(new IWindsorInstaller[] { Install.Service<IProcessExitHandler, DefaultHandler>((Action<ComponentRegistration<IProcessExitHandler>>)null) });
		}
	}
}