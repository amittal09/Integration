using System;

namespace Vertica.Integration.Infrastructure.Windows
{
	public interface IWindowsFactory
	{
		ITaskScheduler TaskScheduler(string machineName = null);

		IWindowsServices WindowsServices(string machineName = null);
	}
}