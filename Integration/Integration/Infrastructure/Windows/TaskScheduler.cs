using System;
using System.Collections;
using System.Runtime.InteropServices;
using TaskScheduler;

namespace Vertica.Integration.Infrastructure.Windows
{
	internal class TaskScheduler : ITaskScheduler
	{
		private readonly ITaskService _taskService;

		public TaskScheduler(string machineName = null)
		{
			this._taskService = (ITaskService)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("0F87369F-A4E5-4CFC-BD3E-73E6154572DD")));
			if (string.IsNullOrWhiteSpace(machineName))
			{
				this._taskService.Connect(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
				return;
			}
			this._taskService.Connect(machineName, Type.Missing, Type.Missing, Type.Missing);
		}

		private ITaskFolder GetOrCreateFolder(string folder)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			ITaskFolder variable = this._taskService.GetFolder("\\");
			IEnumerator enumerator = variable.GetFolders(0).GetEnumerator();
			bool flag = false;
			ITaskFolder current = null;
			while (enumerator.MoveNext() && !flag)
			{
				current = (ITaskFolder)enumerator.Current;
				flag = current.Name.Equals(folder, StringComparison.OrdinalIgnoreCase);
			}
			if (!flag)
			{
				current = variable.CreateFolder(folder, Type.Missing);
			}
			return current;
		}

		public void InstallOrUpdate(ScheduledTaskConfiguration scheduledTask)
		{
			if (scheduledTask == null)
			{
				throw new ArgumentNullException("scheduledTask");
			}
			ITaskFolder orCreateFolder = this.GetOrCreateFolder(scheduledTask.Folder);
			IRegisteredTask task = orCreateFolder.GetTask(scheduledTask.Name);
			ITaskDefinition variable = (task != null ? task.Definition : this._taskService.NewTask(0));
			scheduledTask.Initialize(variable);
			this.InstallOrUpdate(orCreateFolder, variable, scheduledTask);
		}

		private void InstallOrUpdate(ITaskFolder folder, ITaskDefinition task, ScheduledTaskConfiguration configuration)
		{
			folder.RegisterTaskDefinition(configuration.Name, task, Convert.ToInt32(_TASK_CREATION.TASK_CREATE_OR_UPDATE), configuration.Credentials.Username, configuration.Credentials.Password, configuration.GetLogonType(), Type.Missing);
		}

		public void Uninstall(string name, string folder)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			if (string.IsNullOrWhiteSpace(folder))
			{
				throw new ArgumentException("Value cannot be null or empty.", "folder");
			}
			this._taskService.GetFolder(folder).DeleteTask(name, 0);
		}
	}
}