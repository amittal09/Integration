using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class WindowsServiceHandler : IWindowsServiceHandler
	{
		private const string Command = "service";

		private const string ServiceStartMode = "startmode";

		private const string ServiceAccountCommand = "account";

		private const string ServiceAccountUsernameCommand = "username";

		private const string ServiceAccountPasswordCommand = "password";

		private readonly IRuntimeSettings _runtimeSettings;

		private readonly IWindowsServices _windowsServices;

		private static string ExePath
		{
			get
			{
				return Assembly.GetEntryAssembly().Location;
			}
		}

		public static KeyValuePair<string, string> InstallCommand
		{
			get
			{
				return new KeyValuePair<string, string>("service", "install");
			}
		}

		private static IEnumerable<string> ReservedCommandArgs
		{
			get
			{
				yield return "service";
				yield return "startmode";
				yield return "account";
				yield return "username";
				yield return "password";
			}
		}

		public static KeyValuePair<string, string> UninstallCommand
		{
			get
			{
				return new KeyValuePair<string, string>("service", "uninstall");
			}
		}

		public WindowsServiceHandler(IRuntimeSettings runtimeSettings, IWindowsFactory windows)
		{
			if (runtimeSettings == null)
			{
				throw new ArgumentNullException("runtimeSettings");
			}
			this._runtimeSettings = runtimeSettings;
			this._windowsServices = windows.WindowsServices(null);
		}

		private static string ExeArgs(HostArguments args)
		{
			Arguments arguments = new Arguments((
				from x in args.CommandArgs
				where !WindowsServiceHandler.ReservedCommandArgs.Contains<string>(x.Key, StringComparer.OrdinalIgnoreCase)
				select x).Append<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("-service", string.Empty) }).Append<KeyValuePair<string, string>>(args.Args.ToArray<KeyValuePair<string, string>>()).ToArray<KeyValuePair<string, string>>());
			return string.Format("{0} {1}", args.Command, arguments);
		}

		private string GetServiceName(HandleAsWindowsService service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			return Regex.Replace(this.Prefix(service.Name), "\\W", string.Empty);
		}

		public bool Handle(HostArguments args, HandleAsWindowsService service)
		{
			string str;
			System.ServiceProcess.ServiceStartMode serviceStartMode;
			string str1;
			ServiceAccount serviceAccount;
			string str2;
			string str3;
			string str4;
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (!args.CommandArgs.TryGetValue("service", out str4))
			{
				return false;
			}
			Func<KeyValuePair<string, string>, bool> func = (KeyValuePair<string, string> command) => string.Equals(command.Value, str4, StringComparison.OrdinalIgnoreCase);
			if (func(WindowsServiceHandler.InstallCommand))
			{
				WindowsServiceConfiguration windowsServiceConfiguration = (new WindowsServiceConfiguration(this.GetServiceName(service), WindowsServiceHandler.ExePath, WindowsServiceHandler.ExeArgs(args))).DisplayName(this.Prefix(service.DisplayName)).Description(service.Description);
				args.CommandArgs.TryGetValue("startmode", out str);
				if (Enum.TryParse<System.ServiceProcess.ServiceStartMode>(str, true, out serviceStartMode))
				{
					windowsServiceConfiguration.StartMode(serviceStartMode);
				}
				if (!args.CommandArgs.TryGetValue("account", out str1))
				{
					args.CommandArgs.TryGetValue("username", out str2);
					args.CommandArgs.TryGetValue("password", out str3);
					if (!string.IsNullOrWhiteSpace(str2) && !string.IsNullOrWhiteSpace(str3))
					{
						windowsServiceConfiguration.RunAsUser(str2, str3);
					}
				}
				else if (Enum.TryParse<ServiceAccount>(str1, true, out serviceAccount))
				{
					windowsServiceConfiguration.RunAs(serviceAccount);
				}
				this._windowsServices.Install(windowsServiceConfiguration);
			}
			else if (!func(WindowsServiceHandler.UninstallCommand))
			{
				this._windowsServices.Run(this.GetServiceName(service), service.OnStartFactory);
			}
			else
			{
				this._windowsServices.Uninstall(this.GetServiceName(service));
			}
			return true;
		}

		private string Prefix(string value)
		{
			ApplicationEnvironment environment = this._runtimeSettings.Environment;
			return string.Format("Integration Service{0}: {1}", (environment != null ? string.Format(" [{0}]", environment) : string.Empty), value);
		}
	}
}