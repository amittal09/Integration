using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Model
{
	internal class TaskExecutionContext : ITaskExecutionContext
	{
		public Vertica.Integration.Model.Arguments Arguments
		{
			get;
		}

		public ILog Log
		{
			get;
		}

		public TaskExecutionContext(ILog log, Vertica.Integration.Model.Arguments arguments)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			if (arguments == null)
			{
				throw new ArgumentNullException("arguments");
			}
			this.Log = log;
			this.Arguments = arguments;
		}
	}
}