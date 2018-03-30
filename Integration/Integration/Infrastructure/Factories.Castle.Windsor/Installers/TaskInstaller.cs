using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class TaskInstaller : IWindsorInstaller
	{
		private readonly Assembly[] _scanAssemblies;

		private readonly Type[] _addTasks;

		private readonly Type[] _ignoreTasks;

		public TaskInstaller(Assembly[] scanAssemblies, Type[] addTasks, Type[] ignoreTasks)
		{
			this._scanAssemblies = scanAssemblies ?? new Assembly[0];
			this._addTasks = addTasks ?? new Type[0];
			this._ignoreTasks = ignoreTasks ?? new Type[0];
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			Action<ComponentRegistration> action = null;
			foreach (Assembly assembly in this._scanAssemblies.Distinct<Assembly>())
			{
				IWindsorContainer windsorContainer = container;
				IRegistration[] registrationArray = new IRegistration[1];
				BasedOnDescriptor basedOnDescriptor = Classes.FromAssembly(assembly).BasedOn<Task>().Unless(new Predicate<Type>(this._ignoreTasks.Contains<Type>)).Unless(new Predicate<Type>(this._addTasks.Contains<Type>));
				Action<ComponentRegistration> action1 = action;
				if (action1 == null)
				{
					Action<ComponentRegistration> action2 = (ComponentRegistration x) => {
						string str = x.Implementation.TaskName();
						if (container.Kernel.HasComponent(str))
						{
							throw new TaskWithSameNameAlreadyRegistredException(x.Implementation);
						}
						x.Named(str);
					};
					Action<ComponentRegistration> action3 = action2;
					action = action2;
					action1 = action3;
				}
				registrationArray[0] = basedOnDescriptor.Configure(action1).WithServiceDefaultInterfaces();
				windsorContainer.Register(registrationArray);
			}
			foreach (Type type in this._addTasks.Except<Type>(this._ignoreTasks).Distinct<Type>())
			{
				try
				{
					container.Register(new IRegistration[] { Component.For<ITask>().ImplementedBy(type).Named(type.TaskName()) });
				}
				catch (ComponentRegistrationException componentRegistrationException)
				{
					throw new TaskWithSameNameAlreadyRegistredException(type, componentRegistrationException);
				}
			}
		}
	}
}