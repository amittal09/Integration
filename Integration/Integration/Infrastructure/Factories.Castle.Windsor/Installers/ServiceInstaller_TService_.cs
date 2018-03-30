using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class ServiceInstaller<TService> : IWindsorInstaller
	where TService : class
	{
		private readonly Action<ComponentRegistration<TService>> _registration;

		public ServiceInstaller(Action<ComponentRegistration<TService>> registration = null)
		{
			this._registration = registration;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			ComponentRegistration<TService> componentRegistration = Component.For<TService>().ImplementedBy<TService>();
			if (this._registration != null)
			{
				this._registration(componentRegistration);
			}
			container.Register(new IRegistration[] { componentRegistration });
		}
	}
}