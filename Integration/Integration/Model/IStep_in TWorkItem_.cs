using System;

namespace Vertica.Integration.Model
{
	public interface IStep<in TWorkItem> : IStep
	{
		Execution ContinueWith(TWorkItem workItem);

		void Execute(TWorkItem workItem, ITaskExecutionContext context);
	}
}