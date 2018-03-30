using System;
using System.Collections.Generic;

namespace Vertica.Integration.Model
{
	public interface ITask<TWorkItem> : ITask
	{
		IEnumerable<IStep<TWorkItem>> Steps
		{
			get;
		}

		void End(TWorkItem workItem, ITaskExecutionContext context);

		TWorkItem Start(ITaskExecutionContext context);
	}
}