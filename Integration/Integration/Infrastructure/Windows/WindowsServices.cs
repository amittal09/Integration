using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Windows
{
	internal class WindowsServices : IWindowsServices
	{
		private readonly string _machineName;

		public WindowsServices(string machineName = null)
		{
			this._machineName = machineName;
		}

		private static void Dispose(ServiceController[] services)
		{
			ServiceController[] serviceControllerArray = services;
			for (int i = 0; i < (int)serviceControllerArray.Length; i++)
			{
				serviceControllerArray[i].Dispose();
			}
		}

		private void Ensure(string serviceName, ServiceControllerStatus status, Action<ServiceController> action, TimeSpan? timeout)
		{
			this.WithService<object>(serviceName, (ServiceController service) => {
				if (service.Status != status)
				{
					action(service);
					if (!timeout.HasValue)
					{
						service.WaitForStatus(status);
					}
					else
					{
						service.WaitForStatus(status, timeout.Value);
					}
				}
				return new object();
			}, true);
		}

		public bool Exists(string serviceName)
		{
			return this.WithService<bool>(serviceName, (ServiceController service) => service != null, false);
		}

		private IDisposable GetServices(Action<ServiceController[]> services)
		{
			ServiceController[] serviceControllerArray = this.GetServices();
			services(serviceControllerArray);
			return new DisposableAction(() => WindowsServices.Dispose(serviceControllerArray));
		}

		private ServiceController[] GetServices()
		{
			if (string.IsNullOrWhiteSpace(this._machineName))
			{
				return ServiceController.GetServices();
			}
			return ServiceController.GetServices(this._machineName);
		}

		public ServiceControllerStatus GetStatus(string serviceName)
		{
			return this.WithService<ServiceControllerStatus>(serviceName, (ServiceController service) => service.Status, true);
		}

		public void Install(WindowsServiceConfiguration windowsService)
		{
			Func<ServiceController, bool> func2 = null;
			if (windowsService == null)
			{
				throw new ArgumentNullException("windowsService");
			}
			if (!string.IsNullOrWhiteSpace(this._machineName))
			{
				throw new InvalidOperationException("Not supported on remote machines.");
			}
			using (ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller())
			{
				using (ServiceInstaller serviceInstaller = windowsService.CreateInstaller(serviceProcessInstaller))
				{
					if (windowsService.Args != null)
					{
						serviceInstaller.AfterInstall += new InstallEventHandler((object sender, InstallEventArgs installArgs) => {
							ServiceController[] services = this.GetServices();
							Func<ServiceController, bool> u003cu003e9_1 = func2;
							if (u003cu003e9_1 == null)
							{
								Func<ServiceController, bool> func = (ServiceController x) => x.ServiceName.Equals(serviceInstaller.ServiceName);
								Func<ServiceController, bool> func1 = func;
								func2 = func;
								u003cu003e9_1 = func1;
							}
							ServiceController serviceController = ((IEnumerable<ServiceController>)services).SingleOrDefault<ServiceController>(u003cu003e9_1);
							if (serviceController != null)
							{
								WindowsServices.Win32Service.SetServiceArguments(serviceController, windowsService.ExePath, windowsService.Args);
							}
							WindowsServices.Dispose(services);
						});
					}
					serviceInstaller.Install(new Hashtable());
				}
			}
		}

		public void Run(string serviceName, Func<IDisposable> onStartFactory)
		{
			if (string.IsNullOrWhiteSpace(serviceName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "serviceName");
			}
			using (WindowsServiceRunner windowsServiceRunner = new WindowsServiceRunner(serviceName, onStartFactory))
			{
				ServiceBase.Run(windowsServiceRunner);
			}
		}

		public void Start(string serviceName, string[] args = null, TimeSpan? timeout = null)
		{
			this.Ensure(serviceName, ServiceControllerStatus.Running, (ServiceController service) => service.Start(args ?? new string[0]), timeout);
		}

		public void Stop(string serviceName, TimeSpan? timeout = null)
		{
			this.Ensure(serviceName, ServiceControllerStatus.Stopped, (ServiceController service) => service.Stop(), timeout);
		}

		public void Uninstall(string serviceName)
		{
			this.WithService<bool>(serviceName, (ServiceController service) => {
				using (ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller())
				{
					using (ServiceInstaller serviceInstaller = new ServiceInstaller())
					{
						serviceInstaller.Context = new InstallContext(string.Empty, new string[0]);
						serviceInstaller.ServiceName = service.ServiceName;
						serviceInstaller.Parent = serviceProcessInstaller;
						serviceInstaller.Uninstall(null);
					}
				}
				return true;
			}, true);
		}

		private TResult WithService<TResult>(string serviceName, Func<ServiceController, TResult> service, bool throwIfNotFound = true)
		{
			TResult tResult;
			Func<ServiceController, bool> func2 = null;
			if (string.IsNullOrWhiteSpace(serviceName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "serviceName");
			}
			TResult tResult1 = default(TResult);
			using (IDisposable disposable = this.GetServices((ServiceController[] services) => {
				ServiceController[] serviceControllerArray = services;
				Func<ServiceController, bool> u003cu003e9_1 = func2;
				if (u003cu003e9_1 == null)
				{
					Func<ServiceController, bool> func = (ServiceController x) => string.Equals(x.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase);
					Func<ServiceController, bool> func1 = func;
					func2 = func;
					u003cu003e9_1 = func1;
				}
				ServiceController serviceController = ((IEnumerable<ServiceController>)serviceControllerArray).SingleOrDefault<ServiceController>(u003cu003e9_1);
				if (serviceController == null & throwIfNotFound)
				{
					throw new InvalidOperationException(string.Format("Service with name '{0}' does not exist.{1}", serviceName, (!string.IsNullOrWhiteSpace(this._machineName) ? string.Format(" Machine: {0}", this._machineName) : string.Empty)));
				}
				tResult1 = service(serviceController);
			}))
			{
				tResult = tResult1;
			}
			return tResult;
		}

		private static class Win32Service
		{
			private static bool ChangeConfiguration(ServiceController serviceController, string exePathWithArguments)
			{
				return WindowsServices.Win32Service.ChangeServiceConfig(serviceController.ServiceHandle, -1, -1, -1, exePathWithArguments, null, IntPtr.Zero, null, null, null, null) != 0;
			}

			[DllImport("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="ChangeServiceConfigW", ExactSpelling=true, SetLastError=true)]
			private static extern int ChangeServiceConfig(SafeHandle hService, int nServiceType, int nStartType, int nErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, [In] string lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);

			public static void SetServiceArguments(ServiceController serviceController, string exePath, string args)
			{
				exePath = Path.GetFullPath(exePath.Trim(new char[] { ' ', '\'', '\"' }));
				exePath = string.Format("\"{0}\" {1}", exePath, args).TrimEnd(new char[0]);
				if (!WindowsServices.Win32Service.ChangeConfiguration(serviceController, exePath))
				{
					throw new Win32Exception();
				}
			}
		}
	}
}