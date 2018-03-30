using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Runtime.CompilerServices;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class HostFactoryInstaller : IWindsorInstaller
	{
		public HostFactoryInstaller()
		{
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(new IRegistration[] { Component.For<IHostFactory>().UsingFactoryMethod<HostFactoryInstaller.HostFactory>((IKernel kernel) => new HostFactoryInstaller.HostFactory(kernel), false) });
		}

		private class HostFactory : IHostFactory
		{
			private readonly IKernel _kernel;

			public HostFactory(IKernel kernel)
			{
				this._kernel = kernel;
			}

			public IHost[] GetAll()
			{
				return this._kernel.ResolveAll<IHost>();
			}
		}
	}
}