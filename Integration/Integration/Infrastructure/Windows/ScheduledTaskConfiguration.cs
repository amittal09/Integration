using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using TaskScheduler;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class ScheduledTaskConfiguration
	{
		private readonly List<ScheduledTaskAction> _actions;

		private readonly List<ScheduledTaskTrigger> _triggers;

		private string _description;

		public Vertica.Integration.Infrastructure.Windows.Credentials Credentials
		{
			get;
			private set;
		}

		public string Folder
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public ScheduledTaskConfiguration(string name, string folder, string exePath, string args)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			if (string.IsNullOrWhiteSpace(folder))
			{
				throw new ArgumentException("Value cannot be null or empty.", "folder");
			}
			this.Name = name;
			this.Folder = folder;
			this.RunAs(ServiceAccount.LocalService);
			this._actions = new List<ScheduledTaskAction>();
			this._triggers = new List<ScheduledTaskTrigger>();
			this.AddAction(exePath, args);
		}

		public ScheduledTaskConfiguration AddAction(string exePath, string args)
		{
			this._actions.Add(new ScheduledTaskAction(exePath, args));
			return this;
		}

		public ScheduledTaskConfiguration AddTrigger(ScheduledTaskTrigger trigger)
		{
			if (trigger == null)
			{
				throw new ArgumentNullException("trigger");
			}
			this._triggers.Add(trigger);
			return this;
		}

		public ScheduledTaskConfiguration Description(string description)
		{
			this._description = description.NullIfEmpty();
			return this;
		}

		internal _TASK_LOGON_TYPE GetLogonType()
		{
			switch (this.Credentials.Account)
			{
				case ServiceAccount.LocalService:
				case ServiceAccount.NetworkService:
				case ServiceAccount.LocalSystem:
				{
					return _TASK_LOGON_TYPE.TASK_LOGON_SERVICE_ACCOUNT;
				}
				case ServiceAccount.User:
				{
					return _TASK_LOGON_TYPE.TASK_LOGON_PASSWORD;
				}
			}
			return _TASK_LOGON_TYPE.TASK_LOGON_NONE;
		}

		internal void Initialize(ITaskDefinition task)
		{
			if (task == null)
			{
				throw new ArgumentNullException("task");
			}
			task.Settings.MultipleInstances = _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW;
			task.Settings.StopIfGoingOnBatteries = false;
			task.Settings.IdleSettings.StopOnIdleEnd = false;
			task.RegistrationInfo.Description = this._description;
			task.Settings.Hidden = false;
			task.Actions.Clear();
			task.Triggers.Clear();
			foreach (ScheduledTaskAction _action in this._actions)
			{
				IExecAction exePath = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
				exePath.Path = _action.ExePath;
				exePath.Arguments = _action.Args;
			}
			foreach (ScheduledTaskTrigger _trigger in this._triggers)
			{
				_trigger.AddToTask(task);
			}
		}

		public ScheduledTaskConfiguration RunAs(ServiceAccount account)
		{
			this.Credentials = new Vertica.Integration.Infrastructure.Windows.Credentials(account);
			return this;
		}

		public ScheduledTaskConfiguration RunAsUser(string username, string password)
		{
			this.Credentials = new Vertica.Integration.Infrastructure.Windows.Credentials(username, password);
			return this;
		}
	}
}