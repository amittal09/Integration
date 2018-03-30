using System;
using System.Configuration.Install;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class WindowsServiceConfiguration
	{
		private readonly string _serviceName;

		private Credentials _credentials;

		private ServiceStartMode _startMode;

		private string _displayName;

		private string _description;

		public string Args
		{
			get;
			private set;
		}

		public string ExePath
		{
			get;
		}

		public WindowsServiceConfiguration(string serviceName, string exePath, string args = null)
		{
			if (string.IsNullOrWhiteSpace(serviceName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "serviceName");
			}
			if (string.IsNullOrWhiteSpace(exePath))
			{
				throw new ArgumentException("Value cannot be null or empty.", "exePath");
			}
			this._serviceName = serviceName;
			this.ExePath = exePath;
			this.Args = args;
			this.RunAs(ServiceAccount.LocalSystem);
			this.StartMode(ServiceStartMode.Manual);
		}

		internal ServiceInstaller CreateInstaller(ServiceProcessInstaller parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}
			if (this._credentials != null)
			{
				parent.Account = this._credentials.Account;
				parent.Username = this._credentials.Username;
				parent.Password = this._credentials.Password;
			}
			ServiceInstaller serviceInstaller = new ServiceInstaller()
			{
				Context = new InstallContext(string.Empty, new string[] { string.Format("/assemblypath={0}", this.ExePath) }),
				ServiceName = this._serviceName,
				DisplayName = this._displayName ?? this._serviceName,
				Description = this._description ?? (this._displayName ?? this._serviceName),
				StartType = this._startMode,
				Parent = parent
			};
			return serviceInstaller;
		}

		public WindowsServiceConfiguration Description(string description)
		{
			this._description = description.NullIfEmpty();
			return this;
		}

		public WindowsServiceConfiguration DisplayName(string displayName)
		{
			if (string.IsNullOrWhiteSpace(displayName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "displayName");
			}
			this._displayName = displayName;
			return this;
		}

		public WindowsServiceConfiguration RunAs(ServiceAccount account)
		{
			this._credentials = new Credentials(account);
			return this;
		}

		public WindowsServiceConfiguration RunAsUser(string username, string password)
		{
			this._credentials = new Credentials(username, password);
			return this;
		}

		public WindowsServiceConfiguration StartMode(ServiceStartMode startMode)
		{
			this._startMode = startMode;
			return this;
		}
	}
}