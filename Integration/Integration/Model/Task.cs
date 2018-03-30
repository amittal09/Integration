using System;
using System.Linq;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model
{
	public abstract class Task : Task<EmptyWorkItem>
	{
		protected Task() : base(Enumerable.Empty<IStep<EmptyWorkItem>>())
		{
		}

		public static string NameOf<TTask>()
		where TTask : ITask
		{
			return typeof(TTask).TaskName();
		}

		public override EmptyWorkItem Start(ITaskExecutionContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			this.StartTask(context);
			return new EmptyWorkItem();
		}

		public abstract void StartTask(ITaskExecutionContext context);
	}
}