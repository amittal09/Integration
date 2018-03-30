using System;

namespace Vertica.Integration.Model
{
	public abstract class Step<TWorkItem> : IStep<TWorkItem>, IStep
	{
		public abstract string Description
		{
			get;
		}

		protected Step()
		{
		}

		public virtual Execution ContinueWith(TWorkItem workItem)
		{
			return Execution.Execute;
		}

		public abstract void Execute(TWorkItem workItem, ITaskExecutionContext context);
	}
}