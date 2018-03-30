using System;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorTarget : Target
	{
		public System.Net.Mail.MailPriority? MailPriority
		{
			get;
			set;
		}

		public string[] ReceiveErrorsWithMessagesContaining
		{
			get;
			set;
		}

		public string[] Recipients
		{
			get;
			set;
		}

		public MonitorTarget(string name) : base(name)
		{
			this.ReceiveErrorsWithMessagesContaining = new string[0];
		}
	}
}