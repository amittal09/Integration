using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class ScheduledTaskHandler : IScheduledTaskHandler
	{
		private const string Command = "scheduledTask";

		internal const string ServiceAccountCommand = "account";

		internal const string ServiceAccountUsernameCommand = "username";

		internal const string ServiceAccountPasswordCommand = "password";

		private readonly IRuntimeSettings _runtimeSettings;

		private readonly ITaskScheduler _taskScheduler;

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
				return new KeyValuePair<string, string>("scheduledTask", "install");
			}
		}

		private static IEnumerable<string> ReservedCommandArgs
		{
			get
			{
				yield return "scheduledTask";
				yield return "account";
				yield return "username";
				yield return "password";
			}
		}

		public static KeyValuePair<string, string> UninstallCommand
		{
			get
			{
				return new KeyValuePair<string, string>("scheduledTask", "uninstall");
			}
		}

		public ScheduledTaskHandler(IRuntimeSettings runtimeSettings, IWindowsFactory windows)
		{
			this._runtimeSettings = runtimeSettings;
			this._taskScheduler = windows.TaskScheduler(null);
		}

		private static string ExeArgs(HostArguments args)
		{
			Arguments arguments = new Arguments((
				from x in args.CommandArgs
				where !ScheduledTaskHandler.ReservedCommandArgs.Contains<string>(x.Key, StringComparer.OrdinalIgnoreCase)
				select x).Append<KeyValuePair<string, string>>(args.Args.ToArray<KeyValuePair<string, string>>()).ToArray<KeyValuePair<string, string>>());
			return string.Format("{0} {1}", args.Command, arguments);
		}

		private string Folder()
		{
			ApplicationEnvironment environment = this._runtimeSettings.Environment;
			return string.Format("Integration Service{0}", (environment != null ? string.Format(" [{0}]", environment) : string.Empty));
		}

		public bool Handle(HostArguments args, ITask task)
		{
			string str;
			ServiceAccount serviceAccount;
			string str1;
			string str2;
			string str3;
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			if (task == null)
			{
				throw new ArgumentNullException("task");
			}
			if (!args.CommandArgs.TryGetValue("scheduledTask", out str3))
			{
				return false;
			}
			ScheduledTaskConfiguration scheduledTaskConfiguration = (new ScheduledTaskConfiguration(task.Name(), this.Folder(), ScheduledTaskHandler.ExePath, ScheduledTaskHandler.ExeArgs(args))).Description(task.Description);
			Func<KeyValuePair<string, string>, bool> func = (KeyValuePair<string, string> command) => string.Equals(command.Value, str3, StringComparison.OrdinalIgnoreCase);
			if (func(ScheduledTaskHandler.InstallCommand))
			{
				if (!args.CommandArgs.TryGetValue("account", out str))
				{
					args.CommandArgs.TryGetValue("username", out str1);
					args.CommandArgs.TryGetValue("password", out str2);
					if (!string.IsNullOrWhiteSpace(str1) && !string.IsNullOrWhiteSpace(str2))
					{
						scheduledTaskConfiguration.RunAsUser(str1, str2);
					}
				}
				else if (Enum.TryParse<ServiceAccount>(str, out serviceAccount))
				{
					scheduledTaskConfiguration.RunAs(serviceAccount);
				}
				this._taskScheduler.InstallOrUpdate(scheduledTaskConfiguration);
			}
			else if (func(ScheduledTaskHandler.UninstallCommand))
			{
				this._taskScheduler.Uninstall(scheduledTaskConfiguration.Name, scheduledTaskConfiguration.Folder);
			}
			return true;
		}
	}
}