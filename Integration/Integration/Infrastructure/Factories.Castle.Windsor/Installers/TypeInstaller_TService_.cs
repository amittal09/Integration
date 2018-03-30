using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class TypeInstaller<TService> : IWindsorInstaller
	where TService : class
	{
		private readonly Type _type;

		private readonly Action<ComponentRegistration<TService>> _registration;

		public TypeInstaller(Type type, Action<ComponentRegistration<TService>> registration = null)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			this._type = type;
			this._registration = registration;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			ComponentRegistration<TService> componentRegistration = Component.For<TService>().ImplementedBy(this._type);
			if (this._registration != null)
			{
				this._registration(componentRegistration);
			}
			container.Register(new IRegistration[] { componentRegistration });
		}
	}
}