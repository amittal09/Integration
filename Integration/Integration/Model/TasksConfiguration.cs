using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model
{
	public class TasksConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly List<Assembly> _scan;

		private readonly List<Type> _simpleTasks;

		private readonly List<Type> _removeTasks;

		private readonly List<TaskConfiguration> _complexTasks;

		public ApplicationConfiguration Application
		{
			get;
			private set;
		}

		internal TasksConfiguration(ApplicationConfiguration application)
		{
			if (application == null)
			{
				throw new ArgumentNullException("application");
			}
			this.Application = application;
			this._scan = new List<Assembly>();
			this._simpleTasks = new List<Type>();
			this._removeTasks = new List<Type>();
			this._complexTasks = new List<TaskConfiguration>();
			this.AddFromAssemblyOfThis<TasksConfiguration>();
		}

		public TasksConfiguration AddFromAssemblyOfThis<T>()
		{
			this._scan.Add(typeof(T).Assembly);
			return this;
		}

		public TasksConfiguration Clear()
		{
			this._scan.Clear();
			this._simpleTasks.Clear();
			this._removeTasks.Clear();
			return this;
		}

		public TasksConfiguration Remove<TTask>()
		where TTask : Task
		{
			this._removeTasks.Add(typeof(TTask));
			return this;
		}

		public TasksConfiguration Task<TTask, TWorkItem>(Action<TaskConfiguration<TWorkItem>> task = null)
		where TTask : Task<TWorkItem>
		{
			TaskConfiguration<TWorkItem> taskConfiguration = new TaskConfiguration<TWorkItem>(typeof(TTask));
			if (task != null)
			{
				task(taskConfiguration);
			}
			if (this._complexTasks.Contains(taskConfiguration))
			{
				throw new InvalidOperationException(string.Format("Task '{0}' has already been added.", taskConfiguration.Task.FullName));
			}
			this._complexTasks.Add(taskConfiguration);
			return this;
		}

		public TasksConfiguration Task<TTask>()
		where TTask : Task
		{
			this._simpleTasks.Add(typeof(TTask));
			return this;
		}

		void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.Install(new IWindsorInstaller[] { new TaskInstaller(this._scan.ToArray(), this._simpleTasks.ToArray(), this._removeTasks.ToArray()) });
			foreach (TaskConfiguration taskConfiguration in this._complexTasks.Distinct<TaskConfiguration>())
			{
				container.Install(new IWindsorInstaller[] { taskConfiguration.GetInstaller() });
			}
			container.Install(new IWindsorInstaller[] { new TaskFactoryInstaller() });
		}
	}
}