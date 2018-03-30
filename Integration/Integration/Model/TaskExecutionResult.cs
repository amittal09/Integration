using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Model
{
	public class TaskExecutionResult
	{
		public string[] Output
		{
			get;
			private set;
		}

		public TaskExecutionResult(string[] output)
		{
			this.Output = output ?? new string[0];
		}
	}
}