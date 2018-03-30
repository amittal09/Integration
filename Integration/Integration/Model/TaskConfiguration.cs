using Castle.MicroKernel.Registration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Model
{
	public abstract class TaskConfiguration : IEquatable<TaskConfiguration>
	{
		protected List<Type> Steps
		{
			get;
		}

		internal Type Task
		{
			get;
		}

		protected TaskConfiguration(Type task)
		{
			if (task == null)
			{
				throw new ArgumentNullException("task");
			}
			this.Task = task;
			this.Steps = new List<Type>();
		}

		public bool Equals(TaskConfiguration other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			return this.Task == other.Task;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			return this.Equals((TaskConfiguration)obj);
		}

		public override int GetHashCode()
		{
			return this.Task.GetHashCode();
		}

		internal abstract IWindsorInstaller GetInstaller();
	}
}