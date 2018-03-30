using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Infrastructure.Database.Castle.Windsor
{
	internal class DbInstaller<TConnection> : IWindsorInstaller
	where TConnection : Connection
	{
		private readonly TConnection _connection;

		private readonly bool _isDisabled;

		public DbInstaller(TConnection connection, bool isDisabled = false)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			this._connection = connection;
			this._isDisabled = isDisabled;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (container.Kernel.HasComponent(typeof(IDbFactory<TConnection>)))
			{
				throw new InvalidOperationException(string.Format("Only one {0} can be installed.", typeof(TConnection).FullName));
			}
			if (!this._isDisabled)
			{
				container.Register(new IRegistration[] { Component.For<IDbFactory<TConnection>>().UsingFactoryMethod<DbFactory<TConnection>>((IKernel kernel) => new DbFactory<TConnection>(this._connection, kernel), false) });
				return;
			}
			container.Register(new IRegistration[] { Component.For<IDbFactory<TConnection>>().UsingFactoryMethod<IDbFactory<TConnection>>((IKernel kernel, ComponentModel model, CreationContext context) => {
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("DbFactory has been disabled.");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Examine the DependencyChain below to see which component has a dependency of this:");
				stringBuilder.AppendLine();
				context.BuildCycleMessageFor(context.Handler, stringBuilder);
				throw new DatabaseDisabledException(stringBuilder.ToString());
			}, false) });
		}
	}
}