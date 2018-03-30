using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class TaskFactoryInstaller : IWindsorInstaller
	{
		public TaskFactoryInstaller()
		{
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(new IRegistration[] { Component.For<ITaskFactory>().UsingFactoryMethod<TaskFactoryInstaller.TaskFactory>((IKernel kernel) => new TaskFactoryInstaller.TaskFactory(kernel), false) });
		}

		private class TaskFactory : ITaskFactory
		{
			private readonly IKernel _kernel;

			public TaskFactory(IKernel kernel)
			{
				this._kernel = kernel;
			}

			public bool Exists(string name)
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException("Value cannot be null or empty.", "name");
				}
				IHandler handler = this._kernel.GetHandler(name);
				if (handler == null)
				{
					return false;
				}
				return typeof(ITask).IsAssignableFrom(handler.ComponentModel.Implementation);
			}

			public ITask Get<TTask>()
			where TTask : class, ITask
			{
				return this.Get(Task.NameOf<TTask>());
			}

			public ITask Get(string name)
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException("Value cannot be null or empty.", name);
				}
				if (!this.Exists(name))
				{
					throw new TaskNotFoundException(name);
				}
				return this._kernel.Resolve<ITask>(name);
			}

			public ITask[] GetAll()
			{
				return (
					from x in (IEnumerable<ITask>)this._kernel.ResolveAll<ITask>()
					orderby x.Name()
					select x).ToArray<ITask>();
			}

			public bool TryGet(string name, out ITask task)
			{
				task = null;
				if (this.Exists(name))
				{
					task = this.Get(name);
				}
				return task != null;
			}
		}
	}
}