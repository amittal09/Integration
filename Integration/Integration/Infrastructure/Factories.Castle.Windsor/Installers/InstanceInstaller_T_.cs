using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class InstanceInstaller<T> : IWindsorInstaller
	where T : class
	{
		private readonly T _instance;

		public InstanceInstaller(T instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			this._instance = instance;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(new IRegistration[] { Component.For<T>().Instance(this._instance) });
		}
	}
}