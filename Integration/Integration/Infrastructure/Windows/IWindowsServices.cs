using System;
using System.ServiceProcess;

namespace Vertica.Integration.Infrastructure.Windows
{
	public interface IWindowsServices
	{
		bool Exists(string serviceName);

		ServiceControllerStatus GetStatus(string serviceName);

		void Install(WindowsServiceConfiguration windowsService);

		void Run(string serviceName, Func<IDisposable> onStartFactory);

		void Start(string serviceName, string[] args = null, TimeSpan? timeout = null);

		void Stop(string serviceName, TimeSpan? timeout = null);

		void Uninstall(string serviceName);
	}
}