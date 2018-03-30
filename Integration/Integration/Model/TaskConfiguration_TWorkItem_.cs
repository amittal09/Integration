using Castle.MicroKernel.Registration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model
{
	public sealed class TaskConfiguration<TWorkItem> : TaskConfiguration
	{
		internal TaskConfiguration(Type task) : base(task)
		{
		}

		public TaskConfiguration<TWorkItem> Clear()
		{
			base.Steps.Clear();
			return this;
		}

		internal override IWindsorInstaller GetInstaller()
		{
			return new TaskInstaller<TWorkItem>(base.Task, base.Steps);
		}

		public TaskConfiguration<TWorkItem> Remove<TStep>()
		where TStep : IStep<TWorkItem>
		{
			base.Steps.RemoveAll((Type x) => x == typeof(TStep));
			return this;
		}

		public TaskConfiguration<TWorkItem> Step<TStep>()
		where TStep : IStep<TWorkItem>
		{
			base.Steps.Add(typeof(TStep));
			return this;
		}
	}
}