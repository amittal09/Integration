using System;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class ErrorLog
	{
		public string CommandLine
		{
			get;
			private set;
		}

		public string FormattedMessage
		{
			get;
			private set;
		}

		public string Id
		{
			get;
			internal set;
		}

		public string IdentityName
		{
			get;
			private set;
		}

		public string MachineName
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public Vertica.Integration.Infrastructure.Logging.Severity Severity
		{
			get;
			private set;
		}

		public Vertica.Integration.Infrastructure.Logging.Target Target
		{
			get;
			private set;
		}

		public DateTimeOffset TimeStamp
		{
			get;
			private set;
		}

		private ErrorLog(Vertica.Integration.Infrastructure.Logging.Severity severity, ITarget target)
		{
			this.Severity = severity;
			object service = target;
			if (service == null)
			{
				service = Vertica.Integration.Infrastructure.Logging.Target.Service;
			}
			this.Target = ((ITarget)service).Name;
			this.MachineName = Environment.MachineName;
			this.IdentityName = WindowsUtils.GetIdentityName();
			this.CommandLine = Environment.CommandLine.MaxLength(4000);
			this.TimeStamp = Time.UtcNow;
		}

		public ErrorLog(Vertica.Integration.Infrastructure.Logging.Severity severity, string message, ITarget target) : this(severity, target)
		{
			this.Message = message.MaxLength(4000);
			this.FormattedMessage = message;
		}

		public ErrorLog(Exception exception, ITarget target = null) : this(Vertica.Integration.Infrastructure.Logging.Severity.Error, target)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception");
			}
			this.Message = exception.Message.MaxLength(4000);
			this.FormattedMessage = exception.GetFullStacktrace();
		}
	}
}