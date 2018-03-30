using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	public static class ContainerExtensions
	{
		public static void RegisterInstance<T>(this IWindsorContainer container, T instance)
		where T : class
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			container.Install(new IWindsorInstaller[] { new InstanceInstaller<T>(instance) });
		}
	}
}