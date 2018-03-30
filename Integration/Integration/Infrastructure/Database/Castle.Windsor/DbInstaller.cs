using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Infrastructure.Database.Castle.Windsor
{
	internal class DbInstaller : DbInstaller<DefaultConnection>
	{
		public DbInstaller(DefaultConnection connection) : base(connection, connection.IsDisabled)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (container.Kernel.HasComponent(typeof(IDbFactory)))
			{
				throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");
			}
			base.Install(container, store);
			container.Register(new IRegistration[] { Component.For<IDbFactory>().UsingFactoryMethod<DbFactory>((IKernel kernel) => new DbFactory(kernel.Resolve<IDbFactory<DefaultConnection>>()), false) });
		}
	}
}