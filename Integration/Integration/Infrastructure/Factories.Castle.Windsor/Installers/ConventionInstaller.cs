using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Registration.Lifestyle;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	public class ConventionInstaller : IWindsorInstaller
	{
		private readonly List<Assembly> _assemblies;

		private readonly List<Type> _ignoreTypes;

		internal ConventionInstaller()
		{
			this._assemblies = new List<Assembly>();
			this._ignoreTypes = new List<Type>();
		}

		public ConventionInstaller AddFromAssemblyOfThis<T>()
		{
			this._assemblies.Add(typeof(T).Assembly);
			return this;
		}

		public ConventionInstaller Ignore<T>()
		{
			this._ignoreTypes.Add(typeof(T));
			return this;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			foreach (Assembly assembly in this._assemblies.Distinct<Assembly>())
			{
				container.Register(new IRegistration[] { Classes.FromAssembly(assembly).Pick().If((Type classType) => {
					if (!classType.GetInterfaces().Any<Type>((Type classInterface) => this._assemblies.Contains(classInterface.Assembly)))
					{
						return false;
					}
					return !this._ignoreTypes.Any<Type>((Type ignoreType) => ignoreType.IsAssignableFrom(classType));
				}).WithService.DefaultInterfaces().Configure((ComponentRegistration registration) => ConventionInstaller.SetLifestyle(registration.IsFallback())) });
			}
		}

		private static ComponentRegistration<object> SetLifestyle(ComponentRegistration<object> registration)
		{
			if (Attribute.IsDefined(registration.Implementation, typeof(LifestyleAttribute)))
			{
				return registration;
			}
			return registration.LifeStyle.Singleton;
		}
	}
}