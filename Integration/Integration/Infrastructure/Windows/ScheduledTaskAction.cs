using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class ScheduledTaskAction
	{
		public string Args
		{
			get;
			private set;
		}

		public string ExePath
		{
			get;
			private set;
		}

		public ScheduledTaskAction(string exePath, string args)
		{
			if (string.IsNullOrWhiteSpace(exePath))
			{
				throw new ArgumentException("Value cannot be null or empty.", "exePath");
			}
			this.ExePath = exePath;
			this.Args = args;
		}
	}
}