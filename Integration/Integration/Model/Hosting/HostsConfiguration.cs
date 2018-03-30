using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model.Hosting
{
	public class HostsConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly List<Type> _add;

		private readonly List<Type> _remove;

		public ApplicationConfiguration Application
		{
			get;
			private set;
		}

		internal HostsConfiguration(ApplicationConfiguration application)
		{
			if (application == null)
			{
				throw new ArgumentNullException("application");
			}
			this.Application = application;
			this._add = new List<Type>();
			this._remove = new List<Type>();
			this.Host<TaskHost>();
		}

		public HostsConfiguration Clear()
		{
			this._add.Clear();
			this._remove.Clear();
			return this;
		}

		public HostsConfiguration Host<THost>()
		where THost : IHost
		{
			this._add.Add(typeof(THost));
			return this;
		}

		public HostsConfiguration Remove<THost>()
		where THost : IHost
		{
			this._remove.Add(typeof(THost));
			return this;
		}

		void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.Install(new IWindsorInstaller[] { new HostsInstaller(this._add.ToArray(), this._remove.ToArray()) });
			container.Install(new IWindsorInstaller[] { new HostFactoryInstaller() });
		}
	}
}