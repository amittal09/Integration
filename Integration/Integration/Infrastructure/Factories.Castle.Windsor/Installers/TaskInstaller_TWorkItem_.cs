using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
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
	internal class TaskInstaller<TWorkItem> : IWindsorInstaller
	{
		private readonly Type _task;

		private readonly IEnumerable<Type> _steps;

		public TaskInstaller(Type task, IEnumerable<Type> steps)
		{
			if (task == null)
			{
				throw new ArgumentNullException("task");
			}
			if (steps == null)
			{
				throw new ArgumentNullException("steps");
			}
			this._task = task;
			this._steps = steps;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			try
			{
				container.Register(new IRegistration[] { Component.For(typeof(ITask)).ImplementedBy(this._task).Named(this._task.TaskName()) });
			}
			catch (ComponentRegistrationException componentRegistrationException)
			{
				throw new TaskWithSameNameAlreadyRegistredException(this._task, componentRegistrationException);
			}
			List<string> strs = new List<string>();
			foreach (Type _step in this._steps)
			{
				string str = this._task.TaskName();
				string str1 = _step.StepName();
				Guid guid = Guid.NewGuid();
				string str2 = string.Format("{0}.{1}.{2}", str, str1, guid.ToString("N"));
				container.Register(new IRegistration[] { Component.For<IStep<TWorkItem>>().ImplementedBy(_step).Named(str2) });
				strs.Add(str2);
			}
			container.Kernel.Resolver.AddSubResolver(new TaskInstaller<TWorkItem>.TaskStepsResolver(container.Kernel, this._task, strs.ToArray()));
		}

		private class TaskStepsResolver : ISubDependencyResolver
		{
			private readonly IKernel _kernel;

			private readonly Type _task;

			private readonly string[] _stepNames;

			public TaskStepsResolver(IKernel kernel, Type task, string[] stepNames)
			{
				if (kernel == null)
				{
					throw new ArgumentNullException("kernel");
				}
				if (stepNames == null)
				{
					throw new ArgumentNullException("stepNames");
				}
				this._kernel = kernel;
				this._task = task;
				this._stepNames = stepNames;
			}

			public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
			{
				if (model.Implementation != this._task)
				{
					return false;
				}
				return dependency.TargetItemType == typeof(IEnumerable<IStep<TWorkItem>>);
			}

			public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
			{
				return (
					from step in this._stepNames
					select this._kernel.Resolve<IStep<TWorkItem>>(step)).ToArray<IStep<TWorkItem>>();
			}
		}
	}
}