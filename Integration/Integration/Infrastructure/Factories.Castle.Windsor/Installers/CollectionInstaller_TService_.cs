using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	public class CollectionInstaller<TService> : IWindsorInstaller
	{
		private readonly List<Assembly> _assemblies;

		private readonly List<Type> _ignoreTypes;

		internal CollectionInstaller()
		{
			this._assemblies = new List<Assembly>();
			this._ignoreTypes = new List<Type>();
		}

		public CollectionInstaller<TService> AddFromAssemblyOfThis<T>()
		{
			this._assemblies.Add(typeof(T).Assembly);
			return this;
		}

		public CollectionInstaller<TService> Ignore<T>()
		{
			this._ignoreTypes.Add(typeof(T));
			return this;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			foreach (Assembly assembly in this._assemblies.Distinct<Assembly>())
			{
				container.Register(new IRegistration[] { Classes.FromAssembly(assembly).BasedOn<TService>().WithServiceFromInterface(typeof(TService)).If((Type classType) => !this._ignoreTypes.Any<Type>((Type ignoreType) => ignoreType.IsAssignableFrom(classType))) });
			}
			container.Register(new IRegistration[] { Component.For<IEnumerable<TService>>().UsingFactoryMethod<TService[]>((IKernel kernel) => kernel.ResolveAll<TService>(), false) });
			container.Register(new IRegistration[] { Component.For<TService[]>().UsingFactoryMethod<TService[]>((IKernel kernel) => kernel.ResolveAll<TService>(), false) });
		}
	}
}