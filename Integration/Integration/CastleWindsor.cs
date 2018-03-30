using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration
{
	internal static class CastleWindsor
	{
		public static IWindsorContainer Initialize(ApplicationConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			WindsorContainer windsorContainer = new WindsorContainer();
			windsorContainer.Kernel.AddFacility<TypedFactoryFacility>();
			windsorContainer.Register(new IRegistration[] { Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>() });
			configuration.Extensibility((ExtensibilityConfiguration extensibility) => {
				foreach (IInitializable<IWindsorContainer> initializable in extensibility.OfType<IInitializable<IWindsorContainer>>())
				{
					initializable.Initialize(windsorContainer);
				}
			});
			windsorContainer.Install(new IWindsorInstaller[] { Install.ByConvention.AddFromAssemblyOfThis<ConventionInstaller>().Ignore<IApplicationContext>().Ignore<IHost>().Ignore<ITask>().Ignore<IStep>() });
			return windsorContainer;
		}
	}
}