using System;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class Credentials
	{
		public ServiceAccount Account
		{
			get;
			private set;
		}

		public string Password
		{
			get;
			private set;
		}

		public string Username
		{
			get;
			private set;
		}

		public Credentials(ServiceAccount account)
		{
			if (account == ServiceAccount.User)
			{
				throw new ArgumentException("You must use the constructor that takes username and password when passing a User account.");
			}
			this.Username = (account == ServiceAccount.LocalSystem ? "NT AUTHORITY\\SYSTEM" : string.Format("NT AUTHORITY\\{0}", Regex.Replace(account.ToString(), "[a-z][A-Z]", (Match m) => string.Format("{0} {1}", m.Value[0], m.Value[1]))));
			this.Account = account;
		}

		public Credentials(string username, string password)
		{
			if (string.IsNullOrWhiteSpace(username))
			{
				throw new ArgumentException("Value cannot be null or empty", "username");
			}
			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentException("Value cannot be null or empty", "password");
			}
			this.Username = username;
			this.Password = password;
			this.Account = ServiceAccount.User;
		}
	}
}