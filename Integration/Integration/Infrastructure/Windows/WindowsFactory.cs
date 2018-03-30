using System;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class WindowsFactory : IWindowsFactory
	{
		public WindowsFactory()
		{
		}

		public ITaskScheduler TaskScheduler(string machineName = null)
		{
			return new TaskScheduler(machineName);
		}

		public IWindowsServices WindowsServices(string machineName = null)
		{
			return new WindowsServices(machineName);
		}
	}
}